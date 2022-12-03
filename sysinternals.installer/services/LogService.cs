using System.Diagnostics;
using Newtonsoft.Json;
using sysinternals.installer.services.interfaces;

namespace sysinternals.installer.services;

internal class LogService : ILogService
{
    private readonly string _fileLocation;
    private readonly bool _isDebugMode;

    public LogService(string fileLocation = ".\\sysinternals.installer.log", bool? isDebugMode = false)
    {
        _fileLocation = fileLocation;
        _isDebugMode = isDebugMode ?? false;

        Debug($"initialized LogService({fileLocation})").Wait();
    }

    public async Task Debug(string log, object? obj = null)
    {
        if (_isDebugMode || Environment.GetEnvironmentVariable("ENVIRONMENT") == "DEVELOPMENT")
            await Log(log, "debug", obj);
    }

    public async Task Information(string log, object? obj = null)
    {
        await Log(log, "info", obj);
    }

    public async Task Warning(string log, object? obj = null)
    {
        await Log(log, "warn", obj);
    }

    public async Task Error(string log, object? obj = null)
    {
        await Log(log, "error", obj);
    }

    private async Task Log(string log, string level, object? obj = null)
    {
        if (obj != null)
            log = $"{log}: {JsonConvert.SerializeObject(obj)}";

        log = $"[{level}@{DateTime.Now:G}]: {log}";
        await File.AppendAllLinesAsync(_fileLocation, new[] { log });

        Console.WriteLine(log);
        Trace.WriteLine(log);
    }
}