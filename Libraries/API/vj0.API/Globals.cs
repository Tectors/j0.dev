global using static vj0.Core.Globals;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using vj0.API.Models.GitHub;

namespace vj0.API;

public static class Globals
{
    public static RestClient RestClient { get; } = new(
        new RestClientOptions
        {
            UserAgent = $"{APP_NAME}/{VERSION}"
        },
        configureSerialization: s => s.UseSerializer<JsonNetSerializer>());
    
    public static GitHubAPI GitHub { get; } = new(RestClient);
}
