using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using WebApp.Core;
using WebApp.Core.Helpers;

namespace WebApp.UI
{
    public partial class Program
    {
        public static readonly ApplicationArchitecture Architecture = ApplicationArchitecture.Distributed;
    }
}
