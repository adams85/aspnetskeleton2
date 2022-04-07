using System.Threading;
using Microsoft.Extensions.Hosting;

namespace WebApp.Service.Tests.Helpers;

public class NullHostApplicationLifetime : IHostApplicationLifetime
{
    public static readonly NullHostApplicationLifetime Instance = new NullHostApplicationLifetime();

    private NullHostApplicationLifetime() { }

    public CancellationToken ApplicationStarted => new CancellationToken(true);

    public CancellationToken ApplicationStopping => default;

    public CancellationToken ApplicationStopped => default;

    public void StopApplication() { }
}
