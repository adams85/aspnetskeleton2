using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using static WebApp.Common.ModelConstants;

namespace WebApp.DataAccess.Entities
{
    public class User
    {
        public int Id { get; set; }

        [CaseInsensitive, StringLength(UserNameMaxLength)]
        public string UserName { get; set; } = null!;

        [CaseInsensitive, StringLength(UserEmailMaxLength)]
        public string Email { get; set; } = null!;

        [StringLength(128)]
        public string? Password { get; set; }

        [StringLength(UserCommentMaxLength)]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; }

        public int PasswordFailuresSinceLastSuccess { get; set; }

        public DateTime? LastPasswordFailureDate { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public DateTime? LastLockoutDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [StringLength(24)]
        public string? ConfirmationToken { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        [StringLength(24)]
        public string? PasswordVerificationToken { get; set; }

        public DateTime? PasswordVerificationTokenExpirationDate { get; set; }

        [StringLength(24)]
        public string? JwtRefreshToken { get; set; }

        public DateTime? JwtRefreshTokenExpirationDate { get; set; }

        public ICollection<UserRole>? Roles { get; set; }

        public Profile? Profile { get; set; }

        internal sealed class Configuration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                builder.HasIndex(e => e.UserName).IsUnique();
                builder.HasIndex(e => e.Email).IsUnique();
            }
        }
    }
}
