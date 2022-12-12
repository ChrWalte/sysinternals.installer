using sysinternals.installer.objects;

namespace sysinternals.installer.services.interfaces;

internal interface IToolService
{
    Task<List<Tool>> GetSysinternalsAsync(
        bool forceViaHttps = false,
        int numberOfRetries = 5,
        int millisecondsRetryWait = 60000);

    Task<List<Tool>> GetInstalledSysinternalsAsync(string fileLocation);
}