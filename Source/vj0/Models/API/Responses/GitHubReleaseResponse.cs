using Newtonsoft.Json;

namespace vj0.Models.API.Responses;

public class GitHubReleaseResponse
{
    [JsonProperty("name")] public string Name = null!;
    [JsonProperty("html_url")] public string TagURL = null!;
}