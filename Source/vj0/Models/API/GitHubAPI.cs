using System.Threading.Tasks;
using RestSharp;
using Serilog;
using vj0.Models.API.Base;
using vj0.Models.API.Responses;

namespace vj0.Models.API;

public class GitHubAPI(RestClient client) : APIBase(client, Globals.GITHUB_API_LINK)
{
    public async Task<GitHubReleaseResponse?> GetLatestRelease()
    {
        var response = await ExecuteAsync<GitHubReleaseResponse>("releases/latest", verbose: false);

        if (response is null)
        {
            Log.Information("GetLatestRelease failed");
        }

        return response;
    }
}
