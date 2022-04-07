using System;
using Microsoft.Extensions.Caching.Memory;

namespace WebApp.Service.Infrastructure.Caching;

public class CacheOptions
{
    public static readonly CacheItemPriority DefaultPriority = CacheItemPriority.Normal;

    public static readonly CacheOptions Default = new CacheOptions();

    public TimeSpan? AbsoluteExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public CacheItemPriority? Priority { get; set; }
}
