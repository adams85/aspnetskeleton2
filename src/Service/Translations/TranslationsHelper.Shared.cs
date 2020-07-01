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
                .Cast<IPOEntry>()
                .Select(entry =>
                {
                    TranslationEntryData entryData;

                    if (entry is POPluralEntry || entry.Count > 1)
                        entryData = new TranslationEntryData.Plural { Translations = entry.ToArray() };
                    else if (entry is POSingularEntry || entry.Count == 1)
                        entryData = new TranslationEntryData.Singular { Translation = entry[0] };
                    else
                        return null;

                    entryData.Id = entry.Key.Id;
                    entryData.PluralId = entry.Key.PluralId;
                    entryData.ContextId = entry.Key.ContextId;

                    return entryData;
                })
                .Where(entry => entry != null)
                .ToArray()!
        };

        public static POCatalog ToCatalog(this TranslationCatalogData catalogData) => new POCatalog((catalogData.Entries ?? Enumerable.Empty<TranslationEntryData>())
            .Select(entryData =>
            {
                IPOEntry entry;

                var key = new POKey(entryData.Id, entryData.PluralId, entryData.ContextId);

                if (entryData is TranslationEntryData.Plural pluralEntryData)
                    entry = new POPluralEntry(key, pluralEntryData.Translations ?? Enumerable.Empty<string>());
                else if (entryData is TranslationEntryData.Singular singularEntryData)
                    entry = new POSingularEntry(key) { Translation = singularEntryData.Translation };
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

