using sysinternals.installer.objects;

namespace sysinternals.installer.data.interfaces;

internal interface IToolRepository
{
    Task<List<Tool>> GetSysinternalsViaFtpAsync();
    Task<List<Tool>> GetSysinternalsViaHttpsAsync();
    Task<List<Tool>> GetSysinternalsAlreadyInstalledAsync(string fileLocation);
}