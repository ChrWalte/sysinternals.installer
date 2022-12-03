namespace sysinternals.installer.objects;

internal class Config
{
    public string Version { get; set; } = string.Empty;
    public bool? Debug { get; set; } = false;
    public bool? ForceToolsFromHttps { get; set; } = false;
    public List<string> ToolsToIgnore { get; set; } = new();
}