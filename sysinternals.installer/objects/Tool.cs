namespace sysinternals.installer.objects;

internal class Tool
{
    public string Name { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public MemoryStream? File { get; set; }
}