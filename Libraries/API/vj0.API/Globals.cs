global using static vj0.Core.Globals;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace vj0.API;

public static class Globals
{
    public static RestClient RestClient { get; } = new(
        new RestClientOptions
        {
            UserAgent = $"{Core.Globals.APP_NAME}/{VERSION}"
        },
        configureSerialization: s => s.UseSerializer<JsonNetSerializer>());
}
