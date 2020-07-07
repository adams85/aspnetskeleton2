using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using POTools.Services.Resources;

namespace POTools.Services.Extracting
{
    public class ResourceTextExtractor : ILocalizableTextExtractor
    {
        public IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default)
        {
            ResXFileReader resourceReader;
            using (var reader = new StringReader(content))
                resourceReader = new ResXFileReader(reader);

            return resourceReader.Select(kvp => new LocalizableTextInfo { Id = kvp.Value, Comment = kvp.Key });
        }
    }
}
