using System.Threading;
using Microsoft.Extensions.Hosting;

namespace WebApp.Service.Tests.Helpers;

public class NullHostApplicationLifetime : IHostApplicationLifetime
{
    public static readonly NullHostApplicationLifetime Instance = new();

    private NullHostApplicationLifetime() { }

    public CancellationToken ApplicationStarted => new(true);

    public CancellationToken ApplicationStopping => default;

    public CancellationToken ApplicationStopped => default;

    public void StopApplication() { }
}
