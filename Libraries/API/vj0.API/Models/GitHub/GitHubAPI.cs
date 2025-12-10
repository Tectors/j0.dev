using RestSharp;
using Serilog;
using vj0.API.Models.Base;
using vj0.API.Models.GitHub.Responses;

namespace vj0.API.Models.GitHub;

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
    
    public async Task<GitHubContentsResponse[]?> GetOrsionUmaps()
    {
        var response = await ExecuteAsync<GitHubContentsResponse[]>("https://api.github.com/repos/Orsion/Fortnite/contents/.usmap", useBaseUrl: false, verbose: false);

        if (response is null)
        {
            Log.Information("GetOrsionUmaps failed");
        }

        return response;
    }
}
