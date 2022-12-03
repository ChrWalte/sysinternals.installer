using sysinternals.installer.objects;

namespace sysinternals.installer.services.interfaces;

internal interface IToolService
{
    Task<List<Tool>> GetSysinternalsAsync(bool forceViaHttps = false);
    Task<List<Tool>> GetInstalledSysinternalsAsync(string fileLocation);
}