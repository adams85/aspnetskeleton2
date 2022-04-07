using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApp.Service.Settings;

public interface ISettingsProvider
{
    Task Initialization { get; }

    string? this[string name] { get; }
    IReadOnlyDictionary<string, string?> GetAllSettings();
}
