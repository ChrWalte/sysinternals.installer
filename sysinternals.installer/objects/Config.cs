namespace sysinternals.installer.objects;

internal class Config
{
    public string Version { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 5;
    public int RetryDelayInMilliseconds { get; set; } = 60000;
    public bool? Debug { get; set; } = false;
    public bool? ForceToolsFromHttps { get; set; } = false;
    public List<string> ToolsToIgnore { get; set; } = new();
}