using System.Collections.Generic;
using System.Threading;

namespace POTools.Services.Extracting
{
    public interface ILocalizableTextExtractor
    {
        IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default);
    }
}
