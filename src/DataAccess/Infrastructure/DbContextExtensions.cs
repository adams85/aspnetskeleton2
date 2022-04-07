using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using WebApp.DataAccess.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

public static class DbContextExtensions
{
    public static IDbProperties GetDbProperties(this DbContext context)
    {
        return context.GetInfrastructure().GetRequiredService<IDbProperties>();
    }
}
