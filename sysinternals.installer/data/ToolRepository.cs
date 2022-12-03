using HtmlAgilityPack;
using sysinternals.installer.data.interfaces;
using sysinternals.installer.helpers;
using sysinternals.installer.objects;
using sysinternals.installer.services.interfaces;

namespace sysinternals.installer.data;

internal class ToolRepository : IToolRepository
{
    private readonly ILogService _log;

    private readonly List<string> _toolsToIgnore = new()
    {
        "ctrl2cap.amd.sys",
        "ctrl2cap.nt4.sys",
        "ctrl2cap.nt5.sys",
        "dmon.sys",
        "portmon.cnt"
    };

    // TODO: add sysinternals.installer as a tool from GitHub source, this would allow for it to self-update
    public ToolRepository(IEnumerable<string>? toolsToIgnore, ILogService log)
    {
        _log = log;
        _toolsToIgnore.AddRange(toolsToIgnore ?? new List<string>());

        _log.Debug("initialized ToolRepository(...)").Wait();
    }

    public async Task<List<Tool>> GetSysinternalsViaFtpAsync()
    {
        await _log.Debug("entered GetSysinternalsViaFtpAsync()");

        try
        {
            const string sysinternalsLiveNetworkShare = "\\\\live.sysinternals.com\\tools";
            var toolPaths = Directory.GetFiles(sysinternalsLiveNetworkShare)
                .Select(path => path[..^1].ToLower()).ToList();

            var tools = new List<Tool>();
            foreach (var toolPath in toolPaths)
            {
                var toolName = toolPath.Split('\\').Last();
                if (_toolsToIgnore.Contains(toolName))
                {
                    await _log.Debug($"{toolName} found in _toolsToIgnore, skipping");
                    continue;
                }

                var toolFileBytes = await File.ReadAllBytesAsync(toolPath);
                var toolMemoryStream = new MemoryStream(toolFileBytes);
                var toolFileHash = await HashHelper.GetSha512HashOfDataStreamAsync(toolMemoryStream);
                tools.Add(new Tool
                {
                    Name = toolName,
                    Hash = toolFileHash,
                    File = toolMemoryStream
                });
                await _log.Debug($"{toolName} added to tools");
            }

            await _log.Information($"got these {tools.Count} tools from: {sysinternalsLiveNetworkShare}",
                tools.Select(t => t.Name));

            await _log.Debug("exited GetSysinternalsViaFtpAsync()");
            return tools;
        }
        catch (Exception ex)
        {
            await _log.Error("something went wrong in GetSysinternalsViaFtpAsync()", ex);
            return new List<Tool>();
        }
    }

    public async Task<List<Tool>> GetSysinternalsViaHttpsAsync()
    {
        await _log.Debug("entered GetSysinternalsViaHttpsAsync()");

        try
        {
            const string sysinternalsLiveUrl = "https://live.sysinternals.com/tools";
            var httpGetRequest = new HttpRequestMessage(HttpMethod.Get, sysinternalsLiveUrl);
            httpGetRequest.Headers.Add("User-Agent", "sysinternals.installer");

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(httpGetRequest);

            await using var stream = await httpResponse.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();

            var rawHtml = new HtmlDocument();
            rawHtml.LoadHtml(result);

            var toolLinks = rawHtml.DocumentNode.Descendants()
                .Where(node => node.GetAttributeValue("href", string.Empty) != string.Empty)
                .Select(node => node.GetAttributeValue("href", string.Empty).ToLower())
                .Where(url => !string.IsNullOrWhiteSpace(url) && url.Contains('.'))
                .ToList();

            var tools = new List<Tool>();
            foreach (var toolLink in toolLinks)
            {
                var toolName = toolLink.Split('/').Last();
                if (_toolsToIgnore.Contains(toolName))
                {
                    await _log.Debug($"{toolName} found in _toolsToIgnore, skipping");
                    continue;
                }

                var toolFileBytes = await httpClient.GetByteArrayAsync($"{sysinternalsLiveUrl}/{toolName}");
                var toolMemoryStream = new MemoryStream(toolFileBytes);
                var toolFileHash = await HashHelper.GetSha512HashOfDataStreamAsync(toolMemoryStream);
                tools.Add(new Tool
                {
                    Name = toolName,
                    Hash = toolFileHash,
                    File = toolMemoryStream
                });
                await _log.Debug($"{toolName} added to tools");
            }

            await _log.Information($"got these {tools.Count} tools from: {sysinternalsLiveUrl}",
                tools.Select(t => t.Name));

            await _log.Debug("exited GetSysinternalsViaHttpsAsync()");
            return tools;
        }
        catch (Exception ex)
        {
            await _log.Error("something went wrong in GetSysinternalsViaHttpsAsync()", ex);
            return new List<Tool>();
        }
    }

    public async Task<List<Tool>> GetSysinternalsAlreadyInstalledAsync(string fileLocation)
    {
        await _log.Debug($"entered GetSysinternalsAlreadyInstalledAsync({fileLocation})");
        try
        {
            var toolPaths = Directory.GetFiles(fileLocation)
                .Select(path => path.ToLower()).ToList();

            var tools = new List<Tool>();
            foreach (var toolPath in toolPaths)
            {
                var toolName = toolPath.Replace('/', '\\').Split('\\').Last();
                var toolFileBytes = await File.ReadAllBytesAsync(toolPath);
                var toolMemoryStream = new MemoryStream(toolFileBytes);
                var toolFileHash = await HashHelper.GetSha512HashOfDataStreamAsync(toolMemoryStream);
                tools.Add(new Tool
                {
                    Name = toolName,
                    Hash = toolFileHash,
                    File = toolMemoryStream
                });

                await _log.Debug($"{toolName} added to existing tools");
            }

            await _log.Information($"got these {toolPaths.Count} existing tools from: {fileLocation}",
                tools.Select(t => t.Name));

            await _log.Debug("exited GetSysinternalsViaFtpAsync()");
            return tools;
        }
        catch (Exception ex)
        {
            await _log.Error($"something went wrong in GetSysinternalsAlreadyInstalledAsync({fileLocation})", ex);
            return new List<Tool>();
        }
    }
}