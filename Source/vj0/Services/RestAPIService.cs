using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using RestSharp;

using vj0.Framework;
using vj0.Models.API;
using vj0.Core;
using vj0.Core.Models.API;

namespace vj0.Services;

public class RestAPIService : IService
{
    public readonly CentralAPI Central;
    public readonly EpicGamesAPI EpicGames;
    public readonly GitHubAPI GitHub;
    private RestClient _client => SharedGlobal.RestClient;

    public RestAPIService()
    {
        Central = new CentralAPI(_client);
        EpicGames = new EpicGamesAPI(_client);
        GitHub = new GitHubAPI(_client);
    }

    public string GetUrl(RestRequest request) => _client.BuildUri(request).ToString();

    private async Task<byte[]?> GetBytesAsync(string url)
    {
        var request = new RestRequest(url);
        return await _client.DownloadDataAsync(request);
    }

    public byte[]? GetBytes(string url) => GetBytesAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task<FileInfo?> DownloadFileAsync(string url, string destination)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is null) return null;

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        await File.WriteAllBytesAsync(destination, data);
        return new FileInfo(destination);
    }

    public async Task<FileInfo?> DownloadFileAsync(string url, string destination, Action<float> progressAction)
    {
        using var handler = _client.Options.ConfigureMessageHandler?.Invoke(new HttpClientHandler()) ?? new HttpClientHandler();
        using var httpClient = new HttpClient(handler);

        using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode) return null;

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[8192];
        int bytesRead;
        float totalBytesRead = 0;
        var contentLength = response.Content.Headers.ContentLength ?? -1;

        while ((bytesRead = await responseStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalBytesRead += bytesRead;
            if (contentLength > 0)
            {
                progressAction(totalBytesRead / contentLength);
            }
        }

        return new FileInfo(destination);
    }

    private async Task<FileInfo?> DownloadFileAsync(string url, DirectoryInfo destination)
    {
        var outPath = Path.Combine(destination.FullName, Path.GetFileName(url));
        var data = await _client.DownloadDataAsync(new RestRequest(url));
        if (data is null) return null;

        await File.WriteAllBytesAsync(outPath, data);
        return new FileInfo(outPath);
    }

    public FileInfo? DownloadFile(string url, DirectoryInfo destination) => DownloadFileAsync(url, destination).GetAwaiter().GetResult();

    public string? GetHash(string url)
    {
        var response = _client.Head(new RestRequest(url));
        var hashHeader = response.Headers?.FirstOrDefault(h => h.Name is "Hash");
        return hashHeader?.Value;
    }
}
