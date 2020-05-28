using System;

namespace WebApp.Core.Infrastructure
{
    public sealed class DefaultGuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
