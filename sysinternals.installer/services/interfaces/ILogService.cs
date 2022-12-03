namespace sysinternals.installer.services.interfaces;

internal interface ILogService
{
    Task Debug(string log, object? obj = null);
    Task Information(string log, object? obj = null);
    Task Warning(string log, object? obj = null);
    Task Error(string log, object? obj = null);
}