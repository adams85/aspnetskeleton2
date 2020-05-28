using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.DataAccess.Entities
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        internal sealed class Configuration : IEntityTypeConfiguration<UserRole>
        {
            public void Configure(EntityTypeBuilder<UserRole> builder)
            {
                builder.HasKey(e => new { e.UserId, e.RoleId });

                builder
                    .HasOne(e => e.User)
                    .WithMany(e => e.Roles)
                    .HasForeignKey(e => e.UserId);

                builder
                    .HasOne(e => e.Role)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.RoleId);
            }
        }
    }
}
