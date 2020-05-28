using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.DataAccess.Entities
{
    public class MailQueueItem
    {
        public int Id { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(64)]
        public string MailType { get; set; } = null!;

        public byte[] MailModel { get; set; } = null!;

        internal sealed class Configuration : IEntityTypeConfiguration<MailQueueItem>
        {
            public void Configure(EntityTypeBuilder<MailQueueItem> builder)
            {
                builder.HasIndex(e => e.DueDate);
            }
        }
    }
}
