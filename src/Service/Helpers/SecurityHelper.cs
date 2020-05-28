using System;
using System.IO;
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

        #region Reference Numbers

        private const byte RefNoMagic = 0x7;
        private const int RefNoLength = sizeof(int) + 1;
        // http://stackoverflow.com/questions/2745074/fast-ceiling-of-an-integer-division-in-c-c
        private const int RefNoZBase32Length = (8 * RefNoLength + 5 - 1) / 5;
        private const string RefNoPostfixChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string RefNoFromId(IRandom random, int id, int randomPostfixLength = 0)
        {
            Span<byte> bytes = stackalloc byte[RefNoLength];
            bytes[4] = (byte)(id >> 24 & 0xFF);
            bytes[3] = (byte)(id >> 16 & 0xFF);
            bytes[2] = (byte)(id >> 8 & 0xFF);
            bytes[1] = (byte)(id & 0xFF);
            bytes[0] = Crc8.ComputeChecksum(bytes[1..5]);

            var b = RefNoMagic;
            for (var i = 0; i < RefNoLength; i++)
            {
                b ^= bytes[i];
                bytes[i] = b;
            }
            var result = ZBase32.Encode(bytes);

            if (randomPostfixLength != 0)
            {
                var postfix = new char[randomPostfixLength];
                Span<byte> randomBytes = stackalloc byte[4];
                for (var i = 0; i < randomPostfixLength; i++)
                {
                    random.NextBytes(randomBytes);

                    var randomValue =
                        randomBytes[0] |
                        randomBytes[1] << 8 |
                        randomBytes[2] << 16 |
                        (randomBytes[3] & 0x7F) << 24;

                    randomValue %= RefNoPostfixChars.Length;

                    postfix[i] = RefNoPostfixChars[randomValue];
                }
                result += new string(postfix);
            }

            return result;
        }

        public static int IdFromRefNo(string refNo)
        {
            var bytes = ZBase32.Decode(refNo.Substring(0, RefNoZBase32Length));

            var b = RefNoMagic;
            for (var i = 0; i < RefNoLength; i++)
            {
                bytes[i] = (byte)(b ^ bytes[i]);
                b ^= bytes[i];
            }

            if (bytes[0] != Crc8.ComputeChecksum(bytes[1..5]))
                throw new InvalidDataException();

            return
                bytes[1] |
                bytes[2] << 8 |
                bytes[3] << 16 |
                bytes[4] << 24;
        }

        #endregion
    }
}
