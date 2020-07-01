using System.Collections.Generic;
using System.Threading.Tasks;
using Karambolo.PO;

namespace WebApp.Service.Translations
{
    public interface ITranslationsProvider
    {
        Task Initialization { get; }

        IReadOnlyDictionary<(string Location, string Culture), POCatalog> GetCatalogs();
    }
}
