using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using WebApp.Core.Infrastructure;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Users;

namespace WebApp.Api.Infrastructure.Security
{
    public sealed class ApiSecurityService : IApiSecurityService
    {
        internal const string DefaultJwtAudience = "Client";
        internal const string JwtAccessTokenHttpHeaderName = "X-Access-Token";
        internal const string JwtRefreshTokenHttpHeaderName = "X-Refresh-Token";
        internal const string JwtRefreshTokenAuthorizationHeaderParamName = "Refresh";

        private static readonly object s_jwtRefreshTokenHttpContextItemKey = new object();

        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IGuidProvider _guidProvider;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        private readonly TokenValidationParameters _jwtValidationParameters;
        private readonly SigningCredentials _jwtIssuerSigningCredentials;
        private readonly TimeSpan _jwtAccessTokenExpirationTime;
        private readonly TimeSpan _jwtRefreshTokenExpirationTime;

        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public ApiSecurityService(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, IGuidProvider guidProvider, IClock clock,
            IOptions<ApiSecurityOptions>? options, ILogger<ApiSecurityService>? logger)
        {
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? (ILogger)NullLogger.Instance;

            var optionsValue = options?.Value;

            var jwtIssuerSigningKeyValue = optionsValue?.JwtIssuerSigningKey;

            SecurityKey jwtIssuerSigningKey;
            if (string.IsNullOrEmpty(jwtIssuerSigningKeyValue))
            {
                _logger.LogWarning($"{ApiSecurityOptions.DefaultSectionName}:{nameof(ApiSecurityOptions.JwtIssuerSigningKey)} was not configured. A temporary key is generated but be aware that the JWT tokens issued will not be accepted across multiple executions of the application.");
                jwtIssuerSigningKey = new RsaSecurityKey(RSAHelper.GenerateParameters());
            }
            else
                jwtIssuerSigningKey = new RsaSecurityKey(RSAHelper.DeserializeParameters(jwtIssuerSigningKeyValue));

            var jwtAccessTokenClockSkew = optionsValue?.JwtAccessTokenClockSkew ?? ApiSecurityOptions.DefaultJwtAccessTokenClockSkew;

            _jwtValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = jwtIssuerSigningKey,
                ValidAudience = DefaultJwtAudience,
                ValidIssuer = Program.ApplicationName,
                // When receiving a token, check that we've signed it.
                ValidateIssuerSigningKey = true,
                // When receiving a token, check that it is still valid.
                ValidateLifetime = true,
                // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                // machines which should have synchronised time, this can be set to zero.
                ClockSkew = jwtAccessTokenClockSkew,
            };

            _jwtIssuerSigningCredentials = new SigningCredentials(jwtIssuerSigningKey, SecurityAlgorithms.RsaSha256Signature);
            _jwtAccessTokenExpirationTime = optionsValue?.JwtAccessTokenExpirationTime ?? ApiSecurityOptions.DefaultJwtAccessTokenExpirationTime;
            _jwtRefreshTokenExpirationTime = optionsValue?.JwtRefreshTokenExpirationTime ?? ApiSecurityOptions.DefaultJwtRefreshTokenExpirationTime;

            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task<CachedUserInfoData?> GetCachedUserInfo(string userName, bool registerActivity, CancellationToken cancellationToken)
        {
            var result = await _queryDispatcher.DispatchAsync(new GetCachedUserInfoQuery { UserName = userName }, cancellationToken);

            if (result == null)
                return null;

            if (registerActivity)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = result.UserName,
                    SuccessfulLogin = null,
                    UIActivity = false,
                }, CancellationToken.None);

            return result;
        }

        public void ConfigureCookieAuthentication(CookieAuthenticationOptions options)
        {
            options.ForwardChallenge = ApiAuthenticationSchemes.JwtBearer;
            options.ForwardForbid = ApiAuthenticationSchemes.JwtBearer;
            options.Events = new CustomCookieAuthenticationEvents(this, ApiAuthenticationSchemes.Cookie);
        }

        public void ConfigureJwtBearerAuthentication(JwtBearerOptions options)
        {
            var jwtValidationParameters = _jwtValidationParameters.Clone();
            // To support refresh tokens, we have to validate the token lifetime manually.
            jwtValidationParameters.ValidateLifetime = false;

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = jwtValidationParameters;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ExtractJwtRefreshTokenAsync,
                OnTokenValidated = ValidateJwtTokenAsync,
                OnChallenge = context =>
                {
                    // when both cookie and jwt authentication is allowed, challenge logic of jwt will be executed twice
                    // because cookie's challenge is forwarded to jwt (see ConfigureCookieAuthentication),
                    // so we need to detect this to prevent WWW-Authenticate header from being added twice
                    if (context.HttpContext.Response.StatusCode == StatusCodes.Status401Unauthorized)
                        context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
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
                httpContext.Response.Headers.Add(JwtAccessTokenHttpHeaderName, accessToken);
                httpContext.Response.Headers.Add(JwtRefreshTokenHttpHeaderName, refreshToken);
                return Task.CompletedTask;
            }, (userName, refreshToken, httpContext));
        }

        private Task<AuthenticateUserResult> ValidateCredentialsAsync(NetworkCredential credentials, CancellationToken cancellationToken)
        {
            return _queryDispatcher.DispatchAsync(new AuthenticateUserQuery
            {
                UserName = credentials.UserName,
                Password = credentials.Password
            }, cancellationToken);
        }

        private async Task<string> UpdateRefreshTokenAsync(string userName, HttpContext httpContext)
        {
            string? newRefreshToken = null;

            await _commandDispatcher.DispatchAsync(new UpdateJwtRefreshTokenCommand
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
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
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

        private async Task<string?> TryUpdateRefreshTokenAsync(string userName, string refreshToken, HttpContext httpContext)
        {
            string? newRefreshToken = null;

            try
            {
                await _commandDispatcher.DispatchAsync(new UpdateJwtRefreshTokenCommand
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
                return null;
            }

            return newRefreshToken ?? throw new InvalidOperationException();
        }

        private Task ExtractJwtRefreshTokenAsync(MessageReceivedContext context)
        {
            // we accept refresh tokens in the Authorization header as an appendix for security reasons
            // (Authorization headers are usually dropped by clients on redirection automatically as opposed to e.g. a custom header)

            string authorization = context.HttpContext.Request.Headers[HeaderNames.Authorization];

            int separatorIndex;
            if (authorization == null ||
                !(authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) ||
                (separatorIndex = authorization.LastIndexOf(';')) < 0)
                return Task.CompletedTask;

            int i, n;
            // skips possible whitespace between separator and refresh token
            for (i = separatorIndex + 1, n = authorization.Length; i < n; i++)
                if (!char.IsWhiteSpace(authorization[i]))
                    break;

            // checks for param name and equal sign
            if (string.Compare(authorization, i, JwtRefreshTokenAuthorizationHeaderParamName, 0, JwtRefreshTokenAuthorizationHeaderParamName.Length, StringComparison.OrdinalIgnoreCase) != 0 ||
                (i += JwtRefreshTokenAuthorizationHeaderParamName.Length) >= authorization.Length || authorization[i] != '=')
                return Task.CompletedTask;

            var refreshToken = authorization.Substring(i + 1);

            // skips possible whitespace between separator and access token
            for (i = separatorIndex - 1; i >= 0; i--)
                if (!char.IsWhiteSpace(authorization[i]))
                    break;

            // context.Properties.SetParameter(...) seems to be the proper way to pass the refresh token to ValidateJwtTokenAsync but it won't work for some reason...
            context.HttpContext.Items[s_jwtRefreshTokenHttpContextItemKey] = refreshToken;
            context.HttpContext.Request.Headers[HeaderNames.Authorization] = authorization.Substring(0, i + 1);

            return Task.CompletedTask;
        }

        private async Task ValidateJwtTokenAsync(TokenValidatedContext context)
        {
            var jwtToken = (JwtSecurityToken)context.SecurityToken;
            var expires = (jwtToken.Payload.Exp == null) ? null : new DateTime?(jwtToken.ValidTo);
            var notBefore = (jwtToken.Payload.Nbf == null) ? null : new DateTime?(jwtToken.ValidFrom);

            string? refreshToken = null;

            try { Validators.ValidateLifetime(notBefore, expires, jwtToken, _jwtValidationParameters); }
            catch (SecurityTokenValidationException ex)
            {
                if (!(ex is SecurityTokenExpiredException) || (refreshToken = context.HttpContext.Items[s_jwtRefreshTokenHttpContextItemKey] as string) == null)
                {
                    context.Fail(ex);
                    return;
                }
            }

            var identity = (ClaimsIdentity?)context.Principal.Identity!;
            var userName = identity.Name;

            CachedUserInfoData? userInfo;
            try { userInfo = await GetCachedUserInfo(userName, registerActivity: true, context.HttpContext.RequestAborted); }
            catch { userInfo = null; }

            if (userInfo == null || !userInfo.LoginAllowed)
            {
                context.Fail("User does not exist or is not allowed to access the application currently.");
                return;
            }

            if (refreshToken != null && (refreshToken = await TryUpdateRefreshTokenAsync(userName, refreshToken, context.HttpContext)) == null)
            {
                context.Fail("Refresh token is invalid or expired.");
                return;
            }

            identity.AddClaimsFrom(userInfo);

            if (refreshToken != null)
                AddTokensToResponse(userName, refreshToken, context.HttpContext);
        }
    }
}
