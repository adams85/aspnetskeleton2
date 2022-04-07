using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Karambolo.Common;
using Xunit;

namespace WebApp.Service.Proxy.Tests.IntegrationTests;

public class ServiceHostFixture : IAsyncLifetime
{
    internal const string ServiceBaseUrl = "http://localhost:4999";

    private const string ServiceHostCommandEnvironmentVariableName = "WEBAPP_SERVICEHOST_CMD";
    private const string ServiceHostPath = @"src\Service.Host";

    private static string? LocateSolutionRootPath()
    {
        for (string? currentPath = AppContext.BaseDirectory; currentPath != null; currentPath = Path.GetDirectoryName(currentPath))
            {
            if (File.Exists(Path.Combine(currentPath, "WebApp.Distributed.sln")))
                return currentPath;
            }

        return null;
    }

    private static Process StartProcess(string command, string? args, string? workingDir)
    {
        var processInfo = new ProcessStartInfo(command, args!)
        {
            WorkingDirectory = workingDir,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        var process = Process.Start(processInfo) ?? throw new InvalidOperationException("Process could not be started.");

        process.OutputDataReceived += (_, e) => Debug.WriteLine(e.Data);
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (_, e) => Debug.WriteLine(e.Data);
        process.BeginErrorReadLine();

        return process;
    }

    private Process? _serviceHostProcess;

    public async Task InitializeAsync()
    {
        var command = Environment.GetEnvironmentVariable(ServiceHostCommandEnvironmentVariableName);
        string? args, workingDir;
        if (command == null)
        {
            var solutionRootPath = LocateSolutionRootPath();
            if (solutionRootPath == null)
                throw new InvalidOperationException($"Solution root path could not be located. Specify a command which can start the Service.Host application using the environment variable {ServiceHostCommandEnvironmentVariableName}.");

            command = "dotnet";
            args = "build";
            workingDir = Path.Combine(solutionRootPath, ServiceHostPath);

            var buildProcess = StartProcess(command, args, workingDir);

            await buildProcess.WaitForExitAsync().WithTimeout(TimeSpan.FromMinutes(10));

            if (buildProcess.ExitCode != 0)
                throw new InvalidOperationException("The Service.Host application could not be built.");

            args = $"run --no-build Environment=Development Urls={ServiceBaseUrl}";
        }
        else
        {
            args = null;
            workingDir = Environment.CurrentDirectory;
        }

        _serviceHostProcess = StartProcess(command, args, workingDir);

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void StartupListener(object _, DataReceivedEventArgs e)
        {
            if (e.Data?.Contains("Application started. Press Ctrl+C to shut down.") ?? false)
            {
                _serviceHostProcess.OutputDataReceived -= StartupListener;
                tcs.TrySetResult();
            }
        }

        _serviceHostProcess.OutputDataReceived += StartupListener;

        var waitForExitTask = _serviceHostProcess.WaitForExitAsync();
        var task = await Task.WhenAny(tcs.Task.WithTimeout(TimeSpan.FromMinutes(10)), waitForExitTask);

        if (task == waitForExitTask)
            throw new InvalidOperationException("The Service.Host application could not be started.");

        await task;
    }

    public async Task DisposeAsync()
    {
        if (_serviceHostProcess == null)
            return;

        // it'd be nicer to send CTRL+C/SIGTERM but there's no easy way to do that currently
        // https://github.com/dotnet/runtime/issues/14628
        _serviceHostProcess.Kill(entireProcessTree: true);

        await Task.CompletedTask;
    }
}
