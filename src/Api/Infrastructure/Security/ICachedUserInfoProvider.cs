using System.Threading;
using System.Threading.Tasks;
using WebApp.Service.Users;

namespace WebApp.Api.Infrastructure.Security
{
    public interface ICachedUserInfoProvider
    {
        Task<CachedUserInfoData?> GetCachedUserInfo(string userName, bool registerActivity, CancellationToken cancellationToken);
    }
}
