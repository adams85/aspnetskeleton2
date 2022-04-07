using System;
using System.Linq;
using Karambolo.PO;

namespace WebApp.Service.Translations
{
    public static partial class TranslationsHelper
    {
        public static TranslationCatalogData ToData(this POCatalog catalog) => new TranslationCatalogData
        {
            PluralFormCount = catalog.PluralFormCount,
            PluralFormSelector = catalog.PluralFormSelector,
            Entries = catalog
                .AsEnumerable<IPOEntry>()
                .Where(entry => entry.Count > 0)
                .Select(entry =>
                {
                    TranslationEntryData entryData;

                    if (entry.Key.PluralId == null)
                        entryData = new TranslationEntryData.Singular
                        {
                            Id = entry.Key.Id,
                            PluralId = entry.Key.PluralId,
                            ContextId = entry.Key.ContextId,
                            Translation = entry[0]
                        };
                    else
                        entryData = new TranslationEntryData.Plural
                        {
                            Id = entry.Key.Id,
                            PluralId = entry.Key.PluralId,
                            ContextId = entry.Key.ContextId,
                            Translations = entry.ToArray()
                        };

                    return entryData;
                })
                .ToArray()!
        };

        public static POCatalog ToCatalog(this TranslationCatalogData catalogData) => new POCatalog((catalogData.Entries ?? Enumerable.Empty<TranslationEntryData>())
            .Select(entryData =>
            {
                IPOEntry entry;

                var key = new POKey(entryData.Id, entryData.PluralId, entryData.ContextId);

                if (entryData is TranslationEntryData.Singular singularEntryData)
                    entry = new POSingularEntry(key) { Translation = singularEntryData.Translation };
                else if (entryData is TranslationEntryData.Plural pluralEntryData)
                    entry = new POPluralEntry(key, pluralEntryData.Translations ?? Enumerable.Empty<string>());
                else
                    throw new InvalidOperationException();

                return entry;
            }))
        {
            PluralFormCount = catalogData.PluralFormCount,
            PluralFormSelector = catalogData.PluralFormSelector,
        };
    }
}
