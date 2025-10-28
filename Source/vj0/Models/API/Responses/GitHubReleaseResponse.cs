using System.Collections.Generic;
using Newtonsoft.Json;

namespace vj0.Models.API.Responses;

public class GitHubReleaseResponse
{
    [JsonProperty("name")] public string Name = null!;
    [JsonProperty("html_url")] public string TagUrl = null!;
    
    [JsonProperty("assets")] public List<GitHubReleaseAsset> Assets = [];
}

public class GitHubReleaseAsset
{
    [JsonProperty("name")] 
    public string Name = null!;

    [JsonProperty("browser_download_url")] 
    public string DownloadUrl = null!;

    [JsonProperty("content_type")] 
    public string ContentType = null!;

    [JsonProperty("size")] 
    public long Size;

    [JsonProperty("download_count")] 
    public int DownloadCount;
}