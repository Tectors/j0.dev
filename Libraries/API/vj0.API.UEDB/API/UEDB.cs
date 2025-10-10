using RestSharp;

using Serilog;

using vj0.API.Models.Base;
using vj0.API.UEDB.API.Responses;

namespace vj0.API.UEDB.API;

public class UEDB(RestClient client, string gameName) : APIBase(client, "https://uedb.dev/svc/api/v1")
{
    private readonly string _mappingsFolder = Core.Globals.MappingsFolder.FullName;
    
    public async Task<AesResponse?> GetAesAsync(string? url = null, string? version = null, bool useBaseUrl = true)
    {
        url ??= "";
        
        if (useBaseUrl)
        {
            url += $"{gameName}/";
            url += "aes";
        }

        if (!string.IsNullOrWhiteSpace(version))
        {
            url += $"?version={version}";
        }

        var aes = await ExecuteAsync<AesResponse>(url, verbose: false, useBaseUrl: useBaseUrl);

        if (aes is null)
        {
            Log.Information("GetAesAsync failed for URL: {Url}", url);
        }

        return aes;
    }

    public async Task<MappingsResponse?> FetchMappingAsync(string? version = null, bool forceDownload = false, CancellationToken token = default)
    {
        var endpointUrl = $"{gameName}/";
        
        endpointUrl += "mappings";
        if (!string.IsNullOrWhiteSpace(version))
        {
            endpointUrl += $"?version={version}";
        }

        var mappings = await ExecuteAsync<MappingsResponse>(endpointUrl, verbose: false);

        if (mappings is { IsValid: false })
        {
            return null;
        }

        var targetFolder = !string.IsNullOrWhiteSpace(version) ? Path.Combine(_mappingsFolder, version) : _mappingsFolder;
        var mapping = mappings!.GetFirstMapping();
        var (_, url, fileName) = mapping!.Value;
        
        var localPath = Path.Combine(targetFolder, fileName);
        if (forceDownload || !File.Exists(localPath))
        {
            var data = await ExecuteAsync(url, Method.Get, false, useBaseUrl: false);
            if (!data.IsSuccessful || data.RawBytes is null)
            {
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            await File.WriteAllBytesAsync(localPath, data.RawBytes, token);
        }

        mappings.LocalPath = localPath;
        return mappings;
    }
}
