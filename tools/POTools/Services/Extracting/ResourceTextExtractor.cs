using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using POTools.Services.Resources;

namespace POTools.Services.Extracting;

public class ResourceTextExtractor : ILocalizableTextExtractor
{
    public IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default)
    {
        ResXFileReader resourceReader;
        using (var reader = new StringReader(content))
            resourceReader = new ResXFileReader(reader);

        // Select name, value, and comment from the resource reader.
        return resourceReader.Select(item => new LocalizableTextInfo
        {
            Id = item.Value,
            ExtractedComment = $"{(item.Comment is { Length: > 0 } ? item.Comment + " " : string.Empty)}[{item.Name}]",
        });
    }
}
