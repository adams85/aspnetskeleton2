using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApp.Service.Infrastructure;
using WebApp.Service.Users;

namespace WebApp.Api.Infrastructure.Security
{
    public class CustomJwtBearerEvents : JwtBearerEvents
    {
        protected internal const string DefaultJwtAudience = "Client";

        public static void ConfigureOptions<TEvents>(JwtBearerOptions options, IOptions<ApiSecurityOptions>? securityOptions, ILogger<TEvents>? logger)
        {
            var securityOptionsValue = securityOptions?.Value;
            var loggerIntf = logger ?? (ILogger)NullLogger.Instance;

            SecurityKey jwtIssuerSigningKey;
            var jwtIssuerSigningKeyString = securityOptionsValue?.JwtIssuerSigningKey;
            if (string.IsNullOrEmpty(jwtIssuerSigningKeyString))
            {
                loggerIntf.LogWarning($"{ApiSecurityOptions.DefaultSectionName}:{nameof(ApiSecurityOptions.JwtIssuerSigningKey)} was not configured. A temporary key is generated but be aware that the JWT tokens issued will not be accepted across multiple executions of the application.");
                jwtIssuerSigningKey = new RsaSecurityKey(RSAHelper.GenerateParameters());
            }
            else
                jwtIssuerSigningKey = new RsaSecurityKey(RSAHelper.DeserializeParameters(jwtIssuerSigningKeyString));

            var jwtAccessTokenClockSkew = securityOptionsValue?.JwtAccessTokenClockSkew ?? ApiSecurityOptions.DefaultJwtAccessTokenClockSkew;

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters()
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
            options.EventsType = typeof(TEvents);
        }

        public CustomJwtBearerEvents(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            CommandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            QueryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
        }

        protected ICommandDispatcher CommandDispatcher { get; }
        protected IQueryDispatcher QueryDispatcher { get; }

        protected virtual string? GetRefreshToken(TokenValidatedContext context) => null;

        public async Task<CachedUserInfoData?> GetCachedUserInfoAsync(string userName, bool registerActivity, CancellationToken cancellationToken)
        {
            var result = await QueryDispatcher.DispatchAsync(new GetCachedUserInfoQuery { UserName = userName }, cancellationToken);

            if (result == null)
                return null;

            if (registerActivity)
                await CommandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = result.UserName,
                    SuccessfulLogin = null,
                    UIActivity = false,
                }, CancellationToken.None);

            return result;
        }

        protected virtual Task<bool> ValidateRefreshTokenAync(string userName, string refreshToken, HttpContext httpContext) => Task.FromResult(true);

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var jwtToken = (JwtSecurityToken)context.SecurityToken;
            var expires = (jwtToken.Payload.Exp == null) ? null : new DateTime?(jwtToken.ValidTo);
            var notBefore = (jwtToken.Payload.Nbf == null) ? null : new DateTime?(jwtToken.ValidFrom);

            string? refreshToken = null;

            try { Validators.ValidateLifetime(notBefore, expires, jwtToken, context.Options.TokenValidationParameters); }
            catch (SecurityTokenValidationException ex)
            {
                if (!(ex is SecurityTokenExpiredException) || (refreshToken = GetRefreshToken(context)) == null)
                {
                    context.Fail(ex);
                    return;
                }
            }

            var identity = (ClaimsIdentity)context.Principal.Identity;
            var userName = identity.Name;

            CachedUserInfoData? userInfo;
            try { userInfo = await GetCachedUserInfoAsync(userName, registerActivity: true, context.HttpContext.RequestAborted); }
            catch { userInfo = null; }

            if (userInfo == null || !userInfo.LoginAllowed)
            {
                context.Fail("User does not exist or is not allowed to access the application currently.");
                return;
            }

            if (refreshToken != null && !await ValidateRefreshTokenAync(userName, refreshToken, context.HttpContext))
            {
                context.Fail("Refresh token is invalid or expired.");
                return;
            }

            identity.AddClaimsFrom(userInfo);
        }
    }
}
