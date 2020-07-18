using System.Globalization;
using System.Security.Claims;
using System.Threading;

namespace WebApp.Service
{
    public class OperationExecutionContext
    {
        public static readonly OperationExecutionContext Default = new OperationExecutionContext();

        public CultureInfo Culture => CultureInfo.CurrentCulture;
        public CultureInfo UICulture => CultureInfo.CurrentUICulture;

        /// <remarks>
        /// Only authentication type and user name are guaranteed to be available.
        /// Other claims are not forwarded currently in distributed configuration (see CaptureExecutionContextInterceptor and RestoreExecutionContextInterceptor).
        /// </remarks>
        public virtual ClaimsPrincipal? User => Thread.CurrentPrincipal as ClaimsPrincipal;
    }
}
