using System.Text.RegularExpressions;

using Serilog;
using vj0.API.Models.GitHub;
using vj0.API.Models.GitHub.Responses;
using vj0.Plugins.Interfaces;
using vj0.Plugins.Resolvers;
using vj0.Core.Extensions;
using vj0.Core.Framework.Base;
using vj0.Core.Framework.CUEParse;

namespace vj0.Plugins.Fortnite.Resolvers;

public sealed class FortniteArchiveResolverPlugin : IArchiveResolverPlugin, IGameIdPlugin
{
    public string Name => "Fortnite Archive Resolver";
    public string GameId => "Fortnite";
    
    public bool DoesCharacteristicsMatch(BaseProfile Profile)
    {
        return !string.IsNullOrEmpty(Profile.ArchiveDirectory) && Profile.ArchiveDirectory.Contains("Fortnite", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(Profile.Name, @"^\d+(\.\d+){0,2}$");
    }

    public async Task ResolveKeys(BaseProfile Profile)
    {
        var InherentlyMatches = ((IGamePlugin)this).DoesInherentlyMatch(Profile);
        
        const string UEDB_FN_API_URL = $"https://uedb.dev/svc/api/v1/fortnite/aes";
        var GIT_ARCHIVE_URL = $"https://raw.githubusercontent.com/dippyshere/fortnite-aes-archive/refs/heads/master/api/archive/{Profile.Name}.json";

        string API_URL;

        if (InherentlyMatches)
        {
            API_URL = UEDB_FN_API_URL;
        } else {
            if (!StringExtensions.TryParseStringToDouble(Profile.Name, out var value))
            {
                return;
            }
            
            /* Server doesn't actually have proper versioning yet, at the time of writing this */
            /*var useUEDBAPI = value >= 18.00;
            API_URL = useUEDBAPI ? UEDB_FN_API_URL + $"?version={Profile.Name}" : GIT_ARCHIVE_URL;*/
            API_URL = GIT_ARCHIVE_URL;
        }
        
        var aes = await Globals.API.GetAesAsync(API_URL, useBaseUrl: false);

        if (aes is null)
        {
            Log.Information("ResolveKeys Failed");
            
            return;
        }

        if (!string.IsNullOrWhiteSpace(aes.MainKey))
        {
            Profile.Encryption.MainKey = aes.MainKey;
        }

        if (aes.DynamicKeys is not { Count: > 0 })
        {
            return;
        }
        
        Profile.Encryption.Keys.Clear();

        var newKeys = new List<EncryptionKey>();
        var dynamicGUIDs = new HashSet<string>();

        foreach (var key in aes.DynamicKeys.Where(key => EncryptionKey.IsValidKey(key.Key)))
        {
            newKeys.Add(key);
            dynamicGUIDs.Add(key.Guid);
        }

        if (!InherentlyMatches)
        {
            Profile.Encryption.UnknownKeys.AddRange(newKeys);
        }
        else
        {
            Profile.Encryption.Keys.RemoveAll(k => k is null);
            Profile.Encryption.Keys.RemoveAll(k => dynamicGUIDs.Contains(k.Guid));
            Profile.Encryption.Keys.AddRange(newKeys);
        }
    }

    /* This is only done for inherently matched Profiles */
    public async Task ResolveMappings(BaseProfile Profile)
    {
        var InherentlyMatches = ((IGamePlugin)this).DoesInherentlyMatch(Profile);
        if (InherentlyMatches) return;
        
        if (!StringExtensions.TryParseStringToDouble(Profile.Name, out var value))
        {
            return;
        }
        
        if (true /*value >= 15.20*/)
        {
            var AvailableMappings = await API.Globals.GitHub.GetOrsionUmaps();
            if (AvailableMappings is null) return;
            
            foreach (var Release in AvailableMappings)
            {
                if (!Release.Name.Contains("+Release-" + Profile.Name + "-CL", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                var targetFolder = Path.Combine(Core.Globals.MappingsFolder.FullName, Profile.Name);
                
                var downloaded = await Globals.API.DownloadFileAsync(Release.DownloadURL, new DirectoryInfo(targetFolder));
                if (downloaded is null) continue;
                
                Profile.MappingsContainer.Override = true;
                Profile.MappingsContainer.Path = downloaded.FullName;
            }
        }
    }
}
