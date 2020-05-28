using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using static WebApp.Common.ModelConstants;

namespace WebApp.DataAccess.Entities
{
    public class Profile
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [StringLength(UserFirstNameMaxLength)]
        public string? FirstName { get; set; }

        [StringLength(UserLastNameMaxLength)]
        public string? LastName { get; set; }

        [StringLength(UserPhoneNumberMaxLength)]
        public string? PhoneNumber { get; set; }

        internal sealed class Configuration : IEntityTypeConfiguration<Profile>
        {
            public void Configure(EntityTypeBuilder<Profile> builder)
            {
                builder.HasKey(e => e.UserId);

                builder
                    .HasOne(e => e.User)
                    .WithOne(e => e.Profile!)
                    .HasForeignKey<Profile>(e => e.UserId);
            }
        }
    }
}
