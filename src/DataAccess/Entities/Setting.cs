using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.DataAccess.Entities
{
    /// <summary>
    /// User-adjustable application setting.
    /// </summary>
    public partial class Setting
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Value { get; set; }

        public string? DefaultValue { get; set; }

        public string? MinValue { get; set; }

        public string? MaxValue { get; set; }

        internal class Configuration : IEntityTypeConfiguration<Setting>
        {
            public void Configure(EntityTypeBuilder<Setting> builder)
            {
                builder.HasIndex(e => e.Name).IsUnique();
            }
        }
    }
}
