using Newtonsoft.Json;

namespace vj0.API.UEDB.API.Responses;

public class MappingsResponse
{
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("updated")]
    public DateTime Updated { get; set; }

    [JsonProperty("mappings")]
    public Dictionary<string, string> Mappings { get; set; } = new();

    /* Optional local metadata */
    public string? LocalPath { get; set; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Version) &&
        Mappings is { Count: > 0 } &&
        Mappings.Values.All(url => !string.IsNullOrWhiteSpace(url));
    
    public (string Type, string Url, string FileName)? GetFirstMapping()
    {
        if (Mappings is not { Count: > 0 })
        {
            return null;
        }

        var first = Mappings.First();
        var fileName = Path.GetFileName(new Uri(first.Value).AbsolutePath);

        return (first.Key, first.Value, fileName);
    }
}