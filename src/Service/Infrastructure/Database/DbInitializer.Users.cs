using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Roles;
using WebApp.Core;
using WebApp.DataAccess.Entities;
using WebApp.Service.Helpers;

namespace WebApp.Service.Infrastructure.Database
{
    public partial class DbInitializer
    {
        public async Task SeedUsersAsync(CancellationToken cancellationToken)
        {
            string[] usersToLoad = { ApplicationConstants.BuiltInRootUserName };

            var users = await _context.Users
                .Where(entity => usersToLoad.Contains(entity.UserName))
                .AsAsyncEnumerable()
                .ToDictionarySafeAsync(entity => entity.UserName, AsExistingEntity, _caseInsensitiveComparer, cancellationToken).ConfigureAwait(false);

            var adminRole = _context.Roles.Local.First(r => r.RoleName == nameof(RoleEnum.Administators));

            var utcNow = _clock.UtcNow;

            AddOrUpdateUser(users,
                userName: ApplicationConstants.BuiltInRootUserName,
                password: "root",
                email: "root@local",
                firstName: "Built-in Local Adminstrator",
                lastName: null,
                isApproved: true,
                roles: new[] { adminRole },
                utcNow);

            _context.Users.AddRange(GetEntitesToAdd(users.Values));
        }

        private static void AddOrUpdateUser(Dictionary<string, EntityInfo<User>> users, string userName, string password, string email,
            string? firstName, string? lastName, bool isApproved, Role[] roles, DateTime utcNow)
        {
            if (!users.ContainsKey(userName))
                users.Add(userName, AsNewEntity(new User
                {
                    UserName = userName,
                    Password = SecurityHelper.HashPassword(password),
                    Email = email,
                    CreateDate = utcNow,
                    IsApproved = isApproved,
                    LastPasswordChangedDate = utcNow,
                    Profile = new Profile
                    {
                        FirstName = firstName,
                        LastName = lastName,
                    },
                    Roles = roles.Select(role => new UserRole { Role = role }).ToHashSet(),
                }));
        }
    }
}
