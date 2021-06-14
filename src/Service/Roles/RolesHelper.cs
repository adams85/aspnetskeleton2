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
        private static readonly Expression<Func<Role, RoleData>> s_toDataExpression = r => new RoleData
        {
            RoleId = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
        };

        private static readonly Func<Role, RoleData> s_toData = s_toDataExpression.Compile();

        public static RoleData ToData(this Role entity) => s_toData(entity);

        public static IQueryable<RoleData> ToData(this IQueryable<Role> source) => source.Select(s_toDataExpression);

        public static Expression<Func<Role, bool>> GetFilterByNameWhere(string name, bool pattern = false)
        {
            if (pattern)
                return r => r.RoleName.Contains(name);
            else
                return r => r.RoleName == name;
        }

        public static IQueryable<Role> FilterByName(this IQueryable<Role> source, string name, bool pattern = false)
        {
            return source.Where(GetFilterByNameWhere(name, pattern));
        }

        public static Task<Role> GetByNameAsync(this IQueryable<Role> source, string name, CancellationToken cancellationToken)
        {
            return source.FilterByName(name).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
