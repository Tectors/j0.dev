using System.Threading.Tasks;

using RestSharp;

using Serilog;
using vj0.Models.API.Responses;
using vj0.Shared.Models.API.Base;

namespace vj0.Models.API;

public class GitHubAPI(RestClient client) : APIBase(client, GITHUB_API_LINK)
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
