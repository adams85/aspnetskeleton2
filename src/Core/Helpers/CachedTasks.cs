using System.Threading.Tasks;

using static System.Threading.Tasks.Task;

namespace WebApp.Core.Helpers
{
    public static class CachedTasks
    {
        public static class Default<T>
        {
            public static readonly Task<T> Task = FromResult<T>(default!);
        }

        public static class False
        {
            public static readonly Task<bool> Task = FromResult(false);
        }

        public static class True
        {
            public static readonly Task<bool> Task = FromResult(true);
        }
    }
}
