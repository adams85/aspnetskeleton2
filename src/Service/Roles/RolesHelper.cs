using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Roles
{
    internal static class RolesHelper
    {
        private static readonly Expression<Func<Role, RoleData>> s_toDataExpr = r => new RoleData
        {
            RoleId = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
        };

        private static readonly Func<Role, RoleData> s_toData = s_toDataExpr.Compile();

        public static RoleData ToData(this Role entity) => s_toData(entity);

        public static IQueryable<RoleData> ToData(this IQueryable<Role> linq) => linq.Select(s_toDataExpr);

        public static Expression<Func<Role, bool>> GetFilterByNameWhere(string name, bool pattern = false)
        {
            if (pattern)
                return r => r.RoleName.Contains(name);
            else
                return r => r.RoleName == name;
        }

        public static IQueryable<Role> FilterByName(this IQueryable<Role> linq, string name, bool pattern = false)
        {
            return linq.Where(GetFilterByNameWhere(name, pattern));
        }

        public static Task<Role> GetByNameAsync(this IQueryable<Role> linq, string name, CancellationToken cancellationToken)
        {
            return linq.FilterByName(name).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
