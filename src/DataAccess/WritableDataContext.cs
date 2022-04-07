using Microsoft.EntityFrameworkCore;

namespace WebApp.DataAccess;

public class WritableDataContext : DataContext
{
    public WritableDataContext(DbContextOptions<WritableDataContext> options) : base(options) { }
}
