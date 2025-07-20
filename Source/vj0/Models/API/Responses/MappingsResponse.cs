using Newtonsoft.Json;

namespace vj0.Models.API.Responses;

public class MappingsResponse
{
    [JsonProperty("url")] public string Url = string.Empty;
    [JsonProperty("fileName")] public string FileName = string.Empty;

    /* Path where the file was downloaded or found locally */
    public string? LocalPath { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(FileName);
}