using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using static WebApp.Common.ModelConstants;

namespace WebApp.DataAccess.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [CaseInsensitive, StringLength(RoleNameMaxLength)]
        public string RoleName { get; set; } = null!;

        [StringLength(RoleDescriptionMaxLength)]
        public string? Description { get; set; }

        public ICollection<UserRole>? Users { get; set; }

        internal sealed class Configuration : IEntityTypeConfiguration<Role>
        {
            public void Configure(EntityTypeBuilder<Role> builder)
            {
                builder.HasIndex(e => e.RoleName).IsUnique();
            }
        }
    }
}
