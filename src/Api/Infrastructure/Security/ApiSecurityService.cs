using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using WebApp.Core.Infrastructure;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Users;

namespace WebApp.Api.Infrastructure.Security;

public sealed class ApiSecurityService : CustomJwtBearerEvents, IApiSecurityService
{
    internal const string JwtAccessTokenHttpHeaderName = "X-Access-Token";
    internal const string JwtRefreshTokenHttpHeaderName = "X-Refresh-Token";
    internal const string JwtRefreshTokenAuthorizationHeaderParamName = "Refresh";

    private static readonly object s_jwtRefreshTokenHttpContextItemKey = new object();

    private readonly IGuidProvider _guidProvider;
    private readonly IClock _clock;

    private readonly SigningCredentials _jwtIssuerSigningCredentials;
    private readonly TimeSpan _jwtAccessTokenExpirationTime;
    private readonly TimeSpan _jwtRefreshTokenExpirationTime;

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public ApiSecurityService(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, IGuidProvider guidProvider, IClock clock,
        IOptionsMonitor<JwtBearerOptions> jwtBearerOptions, IOptions<ApiSecurityOptions>? options)
        : base(commandDispatcher, queryDispatcher)
    {
        _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));

        var jwtBearerOptionsValue = (jwtBearerOptions ?? throw new ArgumentNullException(nameof(jwtBearerOptions))).Get(ApiAuthenticationSchemes.JwtBearer);
        _jwtIssuerSigningCredentials = new SigningCredentials(jwtBearerOptionsValue.TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);

        var optionsValue = options?.Value;
        _jwtAccessTokenExpirationTime = optionsValue?.JwtAccessTokenExpirationTime ?? ApiSecurityOptions.DefaultJwtAccessTokenExpirationTime;
        _jwtRefreshTokenExpirationTime = optionsValue?.JwtRefreshTokenExpirationTime ?? ApiSecurityOptions.DefaultJwtRefreshTokenExpirationTime;

        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    private string GenerateJwtAccessToken(string userName)
    {
        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, _guidProvider.NewGuid().ToString()),
            // by default JwtRegisteredClaimNames.UniqueName is translated to ClaimTypes.Name on decoding the JWT token
            // see https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/5.5.0/src/System.IdentityModel.Tokens.Jwt/ClaimTypeMapping.cs
            new Claim(JwtRegisteredClaimNames.UniqueName, userName),
        };

        var token = new JwtSecurityToken(
            issuer: Program.ApplicationName,
            audience: DefaultJwtAudience,
            claims,
            expires: _clock.UtcNow + _jwtAccessTokenExpirationTime,
            signingCredentials: _jwtIssuerSigningCredentials);

        return _jwtSecurityTokenHandler.WriteToken(token);
    }

    private void AddTokensToResponse(string userName, string refreshToken, HttpContext httpContext)
    {
        httpContext.Response.OnStarting(state =>
        {
            var (userName, refreshToken, httpContext) = ((string, string, HttpContext))state;
            var accessToken = GenerateJwtAccessToken(userName);
            httpContext.Response.Headers[JwtAccessTokenHttpHeaderName] = accessToken;
            httpContext.Response.Headers[JwtRefreshTokenHttpHeaderName] = refreshToken;
            return Task.CompletedTask;
        }, (userName, refreshToken, httpContext));
    }

    private Task<AuthenticateUserResult> ValidateCredentialsAsync(NetworkCredential credentials, CancellationToken cancellationToken)
    {
        return QueryDispatcher.DispatchAsync(new AuthenticateUserQuery
        {
            UserName = credentials.UserName,
            Password = credentials.Password
        }, cancellationToken);
    }

    private async Task<string> UpdateRefreshTokenAsync(string userName, HttpContext httpContext)
    {
        string? newRefreshToken = null;

        await CommandDispatcher.DispatchAsync(new UpdateJwtRefreshTokenCommand
        {
            UserName = userName,
            TokenExpirationTimeSpan = _jwtRefreshTokenExpirationTime,
            Verify = false,
            OnKeyGenerated = (_, key) => newRefreshToken = (string)key
        }, httpContext.RequestAborted);

        return newRefreshToken ?? throw new InvalidOperationException();
    }

    public async Task<bool> TryIssueJwtTokenAsync(NetworkCredential credentials, HttpContext httpContext)
    {
        var authResult = await ValidateCredentialsAsync(credentials, httpContext.RequestAborted);

        var success = authResult.Status == AuthenticateUserStatus.Successful;

        if (success || authResult.Status == AuthenticateUserStatus.Failed)
        {
            await CommandDispatcher.DispatchAsync(new RegisterUserActivityCommand
            {
                UserName = credentials.UserName,
                SuccessfulLogin = success,
                UIActivity = false,
            }, CancellationToken.None);
        }

        if (!success)
            return false;

        var refreshToken = await UpdateRefreshTokenAsync(credentials.UserName, httpContext);
        AddTokensToResponse(credentials.UserName, refreshToken, httpContext);

        return true;
    }

    public override Task MessageReceived(MessageReceivedContext context)
    {
        // we accept refresh tokens in the Authorization header as an appendix for security reasons
        // (Authorization headers are usually dropped by clients on redirection automatically as opposed to e.g. a custom header)

        string? authorization = context.HttpContext.Request.Headers[HeaderNames.Authorization];

        const string bearerString = "Bearer ";

        int separatorIndex;
        if (authorization == null ||
            !(authorization.StartsWith(bearerString, StringComparison.OrdinalIgnoreCase)) ||
            (separatorIndex = authorization.LastIndexOf(';')) < 0)
        {
            return Task.CompletedTask;
        }

        int i, n;
        // skips possible whitespace between separator and refresh token
        for (i = separatorIndex + 1, n = authorization.Length; i < n; i++)
        {
            if (!char.IsWhiteSpace(authorization[i]))
                break;
        }

        // checks for param name and equal sign
        if (string.Compare(authorization, i, JwtRefreshTokenAuthorizationHeaderParamName, 0, JwtRefreshTokenAuthorizationHeaderParamName.Length, StringComparison.OrdinalIgnoreCase) != 0 ||
            (i += JwtRefreshTokenAuthorizationHeaderParamName.Length) >= authorization.Length || authorization[i] != '=')
        {
            return Task.CompletedTask;
        }

        var refreshToken = authorization.Substring(i + 1);

        // skips possible whitespace between separator and access token
        for (i = separatorIndex - 1; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(authorization[i]))
                break;
        }

        // context.Properties.SetParameter(...) seems to be the proper way to pass the refresh token to TokenValidated but it won't work for some reason...
        context.HttpContext.Items[s_jwtRefreshTokenHttpContextItemKey] = refreshToken;
        context.Token = authorization.Substring(bearerString.Length, i + 1 - bearerString.Length);

        return Task.CompletedTask;
    }

    protected override string? GetRefreshToken(TokenValidatedContext context)
    {
        return context.HttpContext.Items[s_jwtRefreshTokenHttpContextItemKey] as string;
    }

    protected override async Task<bool> ValidateRefreshTokenAync(string userName, string refreshToken, HttpContext httpContext)
    {
        string? newRefreshToken = null;

        try
        {
            await CommandDispatcher.DispatchAsync(new UpdateJwtRefreshTokenCommand
            {
                UserName = userName,
                TokenExpirationTimeSpan = _jwtRefreshTokenExpirationTime,
                Verify = true,
                VerificationToken = refreshToken,
                OnKeyGenerated = (_, key) => newRefreshToken = (string)key
            }, httpContext.RequestAborted);
        }
        catch (ServiceErrorException ex) when (
            ex.ErrorCode == ServiceErrorCode.ParamNotValid &&
            (string)ex.Args[0] == nameof(UpdateJwtRefreshTokenCommand.VerificationToken))
        {
            return false;
        }

        AddTokensToResponse(userName, newRefreshToken ?? throw new InvalidOperationException(), httpContext);

        return true;
    }
}
