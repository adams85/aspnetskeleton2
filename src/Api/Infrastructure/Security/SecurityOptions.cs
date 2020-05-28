using System;

namespace WebApp.Api.Infrastructure.Security
{
    public class SecurityOptions
    {
        public static readonly string DefaultSectionName = "Security";

        public static readonly TimeSpan DefaultJwtAccessTokenClockSkew = TimeSpan.Zero;
        public static readonly TimeSpan DefaultJwtAccessTokenExpirationTime = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultJwtRefreshTokenExpirationTime = TimeSpan.FromDays(7);

        public string? JwtIssuerSigningKey { get; set; }

        public TimeSpan? JwtAccessTokenClockSkew { get; set; }
        public TimeSpan? JwtAccessTokenExpirationTime { get; set; }
        public TimeSpan? JwtRefreshTokenExpirationTime { get; set; }
    }
}
