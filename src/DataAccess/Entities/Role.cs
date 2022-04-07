using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using static WebApp.Common.ModelConstants;

namespace WebApp.DataAccess.Entities;

/// <summary>
/// Role for access control.
/// </summary>
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
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.HasIndex(e => e.RoleName).IsUnique();
        }
    }
}
