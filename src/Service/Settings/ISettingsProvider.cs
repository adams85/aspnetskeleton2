using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Common.Settings;

namespace WebApp.Service.Settings
{
    public interface ISettingsProvider
    {
        Task Initialization { get; }

        string? this[string name] { get; }
        IReadOnlyDictionary<string, string?> GetAllSettings();
    }
}
