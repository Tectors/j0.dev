using RestSharp;

using vj0.API.Models.Base;
using vj0.Plugins.EpicGames.API.Responses;

namespace vj0.Plugins.EpicGames.API;

public class EpicGamesAPI(RestClient client) : APIBase(client)
{
    private const string OAUTH_POST_URL = "https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token";
    private const string OATH_VERIFY_URL = "https://account-public-service-prod.ol.epicgames.com/account/api/oauth/verify";
    private const string BASIC_TOKEN = "basic ZWM2ODRiOGM2ODdmNDc5ZmFkZWEzY2IyYWQ4M2Y1YzY6ZTFmMzFjMjExZjI4NDEzMTg2MjYyZDM3YTEzZmM4NGQ=";

    private async Task<EpicAuthResponse?> GetAuthTokenAsync()
    {
        return await ExecuteAsync<EpicAuthResponse>(OAUTH_POST_URL, Method.Post, parameters:
        [
            new HeaderParameter("Authorization", BASIC_TOKEN),
            new GetOrPostParameter("grant_type", "client_credentials")
        ]);
    }

    public async Task VerifyAuthAsync()
    {
        var auth = await ExecuteAsync<EpicAuthResponse>(OATH_VERIFY_URL, parameters:
        [
            new HeaderParameter("Authorization", $"bearer {Globals.EpicAuth?.Token}")
        ]);

        if (auth is null)
        {
            Globals.EpicAuth = await GetAuthTokenAsync();
        }
    }
}
