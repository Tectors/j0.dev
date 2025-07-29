using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using vj0.Application;
using vj0.Models.API.Base;
using vj0.Models.API.Responses;

namespace vj0.Models.API;

public class CentralAPI(RestClient client) : APIBase(client, "https://fortnitecentral.genxgames.gg")
{
    private readonly string _mappingsFolder = Globals.MappingsFolder.FullName;
    
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

        var response = await ExecuteAsync<JToken>(endpointUrl, verbose: false);
        if (response is null)
        {
            return null;
        }

        var tokens = response.SelectTokens("$.[0].['url','fileName']").ToArray();
        if (tokens.Length == 0)
        {
            return null;
        }

        var url = tokens.ElementAtOrDefault(0)?.ToString();
        var fileName = tokens.ElementAtOrDefault(1)?.ToString() ?? Path.GetFileName(url ?? "");

        var mapping = new MappingsResponse
        {
            Url = url ?? string.Empty,
            FileName = fileName
        };

        if (!mapping.IsValid)
        {
            return null;
        }

        var targetFolder = !string.IsNullOrWhiteSpace(version) ? Path.Combine(_mappingsFolder, version) : _mappingsFolder;

        var localPath = Path.Combine(targetFolder, mapping.FileName);
        if (forceDownload || !File.Exists(localPath))
        {
            var data = await ExecuteAsync(mapping.Url.Replace("https://fortnitecentral.genxgames.gg/", ""), Method.Get, false);
            if (!data.IsSuccessful || data.RawBytes is null)
            {
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            await File.WriteAllBytesAsync(localPath, data.RawBytes, token);
        }

        mapping.LocalPath = localPath;
        return mapping;
    }
}
