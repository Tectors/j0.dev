using System.Net.Http.Headers;

using CUE4Parse.UE4.IO;
using CUE4Parse.Utils;

using Serilog;

using UE4Config.Parsing;

using vj0.Core.Framework.Base;
using vj0.Plugins.Interfaces;
using vj0.Plugins.OnDemand;

namespace vj0.Plugins.Fortnite.OnDemand;

public sealed class FortniteOnDemandPlugin : IOnDemandPlugin, IGameIdPlugin
{
    public string Name => "Fortnite On Demand";
    public string GameId => "Fortnite";
    
    public void PreInitialize()
    {
        _ = EpicGames.Globals.API.VerifyAuthAsync();
    }

    public async Task Initialize(BaseProfile Profile)
    {
        try
        {
            var tocPath = await GetTocPath(Profile);
            if (string.IsNullOrEmpty(tocPath)) return;

            var tocName = tocPath.SubstringAfterLast("/");
            var onDemandFile = new FileInfo(Path.Combine(Core.Globals.OnDemandFolder.FullName, tocName));
            if (!onDemandFile.Exists || onDemandFile.Length == 0)
            {
                /*await RestAPI.DownloadFileAsync($"https://download.epicgames.com/{tocPath}", onDemandFile.FullName);*/
            }

            var options = new IoStoreOnDemandOptions
            {
                ChunkBaseUri = new Uri("https://download.epicgames.com/ias/fortnite/", UriKind.Absolute),
                ChunkCacheDirectory = Core.Globals.OnDemandFolder,
                Authorization = new AuthenticationHeaderValue("Bearer", EpicGames.Globals.EpicAuth?.Token),
                Timeout = TimeSpan.FromSeconds(10)
            };

            var chunkToc = new IoChunkToc(onDemandFile);
            await Profile.Provider.RegisterVfs(chunkToc, options);
            await Profile.Provider.MountAsync();
        }
        catch (Exception)
        {
            Log.Information("Failed to Initialize Texture Streaming");
        }
    }
    
    private static async Task<string> GetTocPath(BaseProfile Profile)
    {
        var onDemandPath = Path.Combine(Profile.ArchiveDirectory, @"..\..\..\Cloud\IoStoreOnDemand.ini");
        if (!File.Exists(onDemandPath)) return string.Empty;

        var onDemandText = await File.ReadAllTextAsync(onDemandPath);
        if (string.IsNullOrWhiteSpace(onDemandText)) return string.Empty;

        var onDemandIni = new ConfigIni();
        onDemandIni.Read(new StringReader(onDemandText));

        return onDemandIni
            .Sections.FirstOrDefault(s => s.Name == "Endpoint")?
            .Tokens.OfType<InstructionToken>()
            .FirstOrDefault(t => t.Key == "TocPath")?
            .Value.Replace("\"", "") ?? string.Empty;
    }
}