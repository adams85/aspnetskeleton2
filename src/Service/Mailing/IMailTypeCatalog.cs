using System.Collections.Generic;

namespace WebApp.Service.Mailing;

internal interface IMailTypeCatalog
{
    IReadOnlyCollection<IMailTypeDefinition> Definitions { get; }

    IMailTypeDefinition? GetDefinition(string mailType, bool throwIfNotFound = false);
}
