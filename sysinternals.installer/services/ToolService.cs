using System.Runtime.InteropServices;
using sysinternals.installer.data.interfaces;
using sysinternals.installer.objects;
using sysinternals.installer.services.interfaces;

namespace sysinternals.installer.services;

internal class ToolService : IToolService
{
    private readonly ILogService _log;
    private readonly IToolRepository _toolRepository;

    public ToolService(IToolRepository toolRepository, ILogService log)
    {
        _toolRepository = toolRepository;
        _log = log;

        _log.Debug("initialized ToolService(...)").Wait();
    }

    public async Task<List<Tool>> GetSysinternalsAsync(bool forceViaHttps = false)
    {
        await _log.Debug("entered GetSysinternalsAsync()");

        var tools = new List<Tool>();
        if (!forceViaHttps && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await _log.Debug("current OS is Windows, getting tools from: _toolRepository.GetSysinternalsViaFtpAsync()");
            tools = await _toolRepository.GetSysinternalsViaFtpAsync();
        }

        if (!tools.Any())
        {
            await _log.Debug("getting tools from: _toolRepository.GetSysinternalsViaHttpsAsync()");
            tools = await _toolRepository.GetSysinternalsViaHttpsAsync();
        }

        await _log.Debug("exited GetSysinternalsAsync()");
        return tools;
    }

    public async Task<List<Tool>> GetInstalledSysinternalsAsync(string fileLocation)
    {
        await _log.Debug("entered GetInstalledSysinternalsAsync()");

        await _log.Debug("getting existingTools from: _toolRepository.GetSysinternalsAlreadyInstalledAsync()");
        var existingTools = await _toolRepository.GetSysinternalsAlreadyInstalledAsync(fileLocation);

        await _log.Debug("exited GetInstalledSysinternalsAsync()");
        return existingTools;
    }
}