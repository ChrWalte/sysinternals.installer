using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Newtonsoft.Json;
using sysinternals.installer.data;
using sysinternals.installer.objects;
using sysinternals.installer.services;

const string toolsDir = "tools";
const string sysinternalsDir = "Sysinternals";
const string pathKey = "Path";

try
{
    if (Directory.Exists(Path.Combine(".", sysinternalsDir)) &&
        Directory.GetFiles(Path.Combine(".", sysinternalsDir)).Any())
        Directory.SetCurrentDirectory(Path.Combine(".", sysinternalsDir));

    var config = new Config
        { Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty };
    if (File.Exists(Path.Combine(".", "sysinternals.updater.config")))
    {
        var configJson = await File.ReadAllTextAsync(Path.Combine(".", "sysinternals.updater.config"));
        config = JsonConvert.DeserializeObject<Config>(configJson);
    }

    // if no .\\tools directory, perform install
    if (!Directory.Exists(Path.Combine(".", toolsDir)))
    {
        // setup
        var log = new LogService(Path.Combine(".", "sysinternals.install.log"));
        await log.Information("performing sysinternals install...");
        var toolRepository = new ToolRepository(new List<string>(), log);
        var toolService = new ToolService(toolRepository, log);
        var tools = await toolService.GetSysinternalsAsync();

        // set up Sysinternals directory
        Directory.CreateDirectory(Path.Combine(".", sysinternalsDir, toolsDir));
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.Copy(Path.Combine(".", "sysinternals.installer.exe"),
                Path.Combine(".", sysinternalsDir, "sysinternals.updater.exe"), true);

            // add tools directory to Path environment variable
            var currentDir = string.Join('\\', Environment.ProcessPath?.Split('\\')[..^1]!);
            var existingPathVariable = Environment.GetEnvironmentVariable(pathKey) ?? string.Empty;
            if (!existingPathVariable.Contains(sysinternalsDir))
                Environment.SetEnvironmentVariable(pathKey,
                    $"{existingPathVariable};{Path.Combine(currentDir, sysinternalsDir, toolsDir)}");

            // set application to run at startup
            var regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            regKey?.SetValue("sysinternals.updater",
                Path.Combine(currentDir, sysinternalsDir, toolsDir, "sysinternals.updater.exe"));
        }
        else
        {
            File.Copy("sysinternals.installer", Path.Combine(".", sysinternalsDir, "sysinternals.updater"), true);
        }

        await File.AppendAllTextAsync(Path.Combine(".", sysinternalsDir, "sysinternals.updater.config"),
            JsonConvert.SerializeObject(config));

        // install all the tools
        foreach (var tool in tools)
            await File.WriteAllBytesAsync(Path.Combine(".", sysinternalsDir, toolsDir, tool.Name), tool.File?.ToArray()
                ?? throw new NullReferenceException());

        // copy sysinternals readme and eula to root directory if it exists
        if (File.Exists(Path.Combine(".", sysinternalsDir, toolsDir, "readme.txt")))
            File.Copy(Path.Combine(".", sysinternalsDir, toolsDir, "readme.txt"),
                Path.Combine(".", sysinternalsDir, "readme.txt"), true);
        if (File.Exists(Path.Combine(".", sysinternalsDir, toolsDir, "eula.txt")))
            File.Copy(Path.Combine(".", sysinternalsDir, toolsDir, "eula.txt"),
                Path.Combine(".", sysinternalsDir, "eula.txt"), true);

        // clean up tool files in memory:
        tools.ForEach(tool => tool.File?.Dispose());
        await log.Debug("cleaned up tool files in memory");
        await log.Information("exited.\n");
    }
    else // if .\\tools directory exists, perform update
    {
        // setup
        var log = new LogService(Path.Combine(".", "sysinternals.update.log"), config?.Debug);
        await log.Information("performing sysinternals update...");
        var toolRepository = new ToolRepository(config?.ToolsToIgnore, log);
        var toolService = new ToolService(toolRepository, log);
        var tools = await toolService.GetSysinternalsAsync(config?.ForceToolsFromHttps ?? false);
        var existingTools = await toolService.GetInstalledSysinternalsAsync(Path.Combine(".", toolsDir));

        // determine tools to install
        var toolsToBeInstalled = new List<Tool>();
        foreach (var tool in tools)
        {
            var existingTool = existingTools.SingleOrDefault(t => t.Name == tool.Name);
            if (existingTool == null)
            {
                toolsToBeInstalled.Add(tool);
                await log.Information($"found tool not installed: {tool.Name}");
            }
            else if (tool.Hash != existingTool.Hash)
            {
                toolsToBeInstalled.Add(tool);
                await log.Information(
                    $"found new version of tool: {tool.Name}: [{string.Join("", existingTool.Hash.Take(10))}]->[{string.Join("", tool.Hash.Take(10))}]");
            }
        }

        // install tools
        if (toolsToBeInstalled.Any())
        {
            foreach (var tool in toolsToBeInstalled)
                await File.WriteAllBytesAsync(Path.Combine(".", toolsDir, tool.Name), tool.File?.ToArray()
                    ?? throw new NullReferenceException());

            // copy sysinternals readme and eula to root directory if newer
            if (toolsToBeInstalled.Any(t => t.Name == "readme.txt"))
                File.Copy(Path.Combine(".", toolsDir, "readme.txt"),
                    Path.Combine(".", "readme.txt"), true);
            if (toolsToBeInstalled.Any(t => t.Name == "eula.txt"))
                File.Copy(Path.Combine(".", toolsDir, "eula.txt"),
                    Path.Combine(".", "eula.txt"), true);
        }
        else
        {
            await log.Information("nothing to be installed, nothing changed.");
        }

        // clean up tool files in memory:
        tools.ForEach(tool => tool.File?.Dispose());
        existingTools.ForEach(tool => tool.File?.Dispose());
        toolsToBeInstalled.ForEach(tool => tool.File?.Dispose());
        await log.Debug("cleaned up tool files in memory");
        await log.Information("exited.\n");
    }
}
catch (Exception ex)
{
    Console.WriteLine("something went wrong...");
    Console.WriteLine(ex.ToString());
    Trace.WriteLine(ex.ToString());
}
finally
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Console.Write("press enter to exit...");
        Console.ReadLine();
    }
}