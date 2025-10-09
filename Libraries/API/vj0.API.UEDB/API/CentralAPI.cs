using Newtonsoft.Json.Linq;

using RestSharp;

using Serilog;

using vj0.API.Models.Base;
using vj0.API.UEDB.API.Responses;

namespace vj0.API.UEDB.API;

public class CentralAPI(RestClient client) : APIBase(client, "https://fortnitecentral.genxgames.gg")
{
    private readonly string _mappingsFolder = Core.Globals.MappingsFolder.FullName;
    
    public async Task<AesResponse?> GetAesAsync(string? url = null, string? version = null, bool useBaseUrl = true)
    {
        url ??= "api/v1/aes";
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
        var endpointUrl = "api/v1/mappings";
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
