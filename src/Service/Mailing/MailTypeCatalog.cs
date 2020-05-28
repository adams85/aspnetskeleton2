using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Service.Mailing
{
    internal sealed class MailTypeCatalog : IMailTypeCatalog
    {
        private readonly Dictionary<string, IMailTypeDefinition> _definitions;

        public MailTypeCatalog(IEnumerable<IMailTypeDefinition> definitions)
        {
            _definitions = definitions.ToDictionary(definition => definition.MailType, definition => definition);
        }

        public IReadOnlyCollection<IMailTypeDefinition> Definitions => _definitions.Values;

        public IMailTypeDefinition? GetDefinition(string mailType, bool throwIfNotFound = false) =>
            _definitions.TryGetValue(mailType, out var definition) ? definition :
            !throwIfNotFound ? (IMailTypeDefinition?)null :
            throw new ArgumentException($"No {nameof(IMailTypeDefinition)} implementation was registered for mail type {mailType}.", nameof(mailType));
    }
}
