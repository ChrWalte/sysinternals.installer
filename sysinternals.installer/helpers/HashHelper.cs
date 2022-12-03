using System.Security.Cryptography;
using System.Text;

namespace sysinternals.installer.helpers;

internal static class HashHelper
{
    public static async Task<string> GetSha512HashOfDataStreamAsync(Stream dataStream)
    {
        // get byte hash of raw data
        using var sha512 = SHA512.Create();
        var hash = await sha512.ComputeHashAsync(dataStream);

        // get string hash from byte hash
        var stringBuilder = new StringBuilder();
        foreach (var b in hash)
            stringBuilder.Append(b.ToString("x2"));

        // return string hash
        return stringBuilder.ToString();
    }
}