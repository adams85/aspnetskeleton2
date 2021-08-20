using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Core.Helpers;
using WebApp.Core.Infrastructure;
using WebApp.DataAccess;
using WebApp.DataAccess.Infrastructure;
using WebApp.DataAccess.Migrations;

namespace WebApp.Service.Infrastructure.Database
{
    public sealed partial class DbInitializer : IApplicationInitializer
    {
        private readonly WritableDataContext _context;
        private readonly IClock _clock;
        private readonly IDbProperties _dbProperties;

        private readonly StringComparer _caseSensitiveComparer;
        private readonly StringComparer _caseInsensitiveComparer;
        private readonly ILogger _logger;

        private readonly bool _dbEnsureCreated;
        private readonly DbSeedObjects _dbSeedObjects;

        public DbInitializer(WritableDataContext context, IOptions<DbInitializerOptions> options, IClock clock, ILogger<DbInitializer>? logger)
        {
            if (options?.Value == null)
                throw new ArgumentNullException(nameof(options));

            _context = context ?? throw new ArgumentNullException(nameof(context));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _dbProperties = _context.GetDbProperties();
            _caseSensitiveComparer = _dbProperties.CaseSensitiveComparer;
            _caseInsensitiveComparer = _dbProperties.CaseInsensitiveComparer;

            _logger = logger ?? (ILogger)NullLogger.Instance;

            _dbEnsureCreated = options.Value.EnsureCreated;
            _dbSeedObjects = options.Value.Seed;
        }

        private bool ShouldSeedObjects(DbSeedObjects value) => (_dbSeedObjects & value) == value;

        public async Task InitializeAsync(bool designTime, CancellationToken cancellationToken)
        {
            if (!designTime && _dbEnsureCreated)
            {
                _logger.LogInformation("Ensuring database...");
                try
                {
                    await _context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while migrating the DB schema to the latest version.");
                    throw;
                }
            }

            if (_dbSeedObjects != DbSeedObjects.None)
            {
                _logger.LogInformation("Seeding database...");
                try
                {
                    await SeedAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while seeding the DB.");
                }
            }
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync(designTime: false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates/updates DB objects which are not supported by EF Core migrations out-of-the-box (triggers, SPs, etc.)
        /// </summary>
        private async Task SeedDbObjectsAsync(CancellationToken cancellationToken)
        {
            var operations = new CustomDbObjects(_context.Model, _context.Database.ProviderName).GetAllDbObjectsOperations(dropIfExists: true, create: true);

            var commands = _context.Database.GenerateMigrationCommands(operations);

            if (commands.Count > 0)
                await _context.Database.ExecuteMigrationCommandsAsync(commands, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            if (ShouldSeedObjects(DbSeedObjects.DbObjects))
                await SeedDbObjectsAsync(cancellationToken).ConfigureAwait(false);

            await using ((await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)).AsAsyncDisposable(out var transaction).ConfigureAwait(false))
            {
                if (ShouldSeedObjects(DbSeedObjects.BaseData))
                {
                    await SeedSettingsAsync(cancellationToken).ConfigureAwait(false);
                    await SeedRolesAsync(cancellationToken).ConfigureAwait(false);
                    await SeedUsersAsync(cancellationToken).ConfigureAwait(false);
                }

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                transaction.Commit();
            }

            return true;
        }

        #region Helper classes and functions

        private enum EntityState
        {
            Unseen,
            New,
            Seen,
        }

        private sealed class EntityInfo<TEntity>
        {
            public EntityInfo(TEntity entity)
            {
                Entity = entity;
            }

            public TEntity Entity { get; }
            public EntityState State { get; set; }
        }

        private static EntityInfo<TEntity> AsExistingEntity<TEntity>(TEntity entity) => new EntityInfo<TEntity>(entity) { State = EntityState.Unseen };

        private static EntityInfo<TEntity> AsNewEntity<TEntity>(TEntity entity) => new EntityInfo<TEntity>(entity) { State = EntityState.New };

        private static IEnumerable<TEntity> GetEntitesToAdd<TEntity>(IEnumerable<EntityInfo<TEntity>> entityInfos) =>
            entityInfos.Where(info => info.State == EntityState.New).Select(info => info.Entity);

        private static IEnumerable<TEntity> GetEntitesToRemove<TEntity>(IEnumerable<EntityInfo<TEntity>> entityInfos) =>
            entityInfos.Where(info => info.State == EntityState.Unseen).Select(info => info.Entity);

        private static IEnumerable<TEntity> GetExistingEntities<TEntity>(IEnumerable<EntityInfo<TEntity>> entityInfos) =>
            entityInfos.Where(info => info.State != EntityState.New).Select(info => info.Entity);

        #endregion
    }
}
