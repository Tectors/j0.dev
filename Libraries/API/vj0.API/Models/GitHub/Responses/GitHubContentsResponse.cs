using Newtonsoft.Json;

namespace vj0.API.Models.GitHub.Responses;

public class GitHubContentsResponse
{
    [JsonProperty("name")] public string Name = null!;
    [JsonProperty("download_url")] public string DownloadURL = null!;
}