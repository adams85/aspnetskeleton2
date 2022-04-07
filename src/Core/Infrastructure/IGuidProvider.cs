using System;

namespace WebApp.Core.Infrastructure;

public interface IGuidProvider
{
    Guid NewGuid();
}
