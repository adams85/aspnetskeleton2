using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebApp.Core.Infrastructure;

namespace WebApp.Service.Helpers
{
    public static class SecurityHelper
    {
        #region Passwords

        private static readonly IPasswordHasher<object> s_passwordHasher = new PasswordHasher<object>(Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
            IterationCount = 10000
        }));

        public static string HashPassword(string password)
        {
            return s_passwordHasher.HashPassword(null!, password);
        }

        public static PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return s_passwordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        }

        #endregion

        #region Tokens

        public static string GenerateToken(IGuidProvider guidProvider)
        {
            Span<byte> tokenBytes = stackalloc byte[16];
            guidProvider.NewGuid().TryWriteBytes(tokenBytes);

            return Convert.ToBase64String(tokenBytes);
        }

        public static string GenerateToken(IRandom random, int tokenSizeInBytes)
        {
            if (tokenSizeInBytes <= 0 || 1024 < tokenSizeInBytes)
                throw new ArgumentOutOfRangeException(nameof(tokenSizeInBytes));

            Span<byte> tokenBytes = stackalloc byte[tokenSizeInBytes];
            random.NextBytes(tokenBytes);

            return Convert.ToBase64String(tokenBytes);
        }

        #endregion
    }
}
