using System;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess
{
    public abstract class DataContext : DbContext
    {
        protected DataContext(DbContextOptions options)
            : base(options) { }

        public virtual DbSet<Setting> Settings { get; set; } = null!;

        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Profile> Profiles { get; set; } = null!;

        public virtual DbSet<MailQueueItem> MailQueue { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        }

        public override void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
            base.Dispose();
        }

        public event EventHandler? Disposing;
    }
}
