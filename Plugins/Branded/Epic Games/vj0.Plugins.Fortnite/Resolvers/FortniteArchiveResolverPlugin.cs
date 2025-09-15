using System.Text.RegularExpressions;

using Serilog;

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
        return Profile.ArchiveDirectory.Contains("Fortnite", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(Profile.Name, @"^\d+(\.\d+){0,2}$");
    }

    public async Task ResolveKeys(BaseProfile Profile)
    {
        var InherentlyMatches = ((IGamePlugin)this).DoesInherentlyMatch(Profile);
        
        const string GEN_API_URL = $"https://fortnitecentral.genxgames.gg/api/v1/aes";
        var GIT_ARCHIVE_URL = $"https://raw.githubusercontent.com/Tectors/fortnite-aes-archive/refs/heads/master/api/archive/{Profile.Name}.json";

        string API_URL;

        if (InherentlyMatches)
        {
            API_URL = GEN_API_URL;
        } else {
            if (!StringExtensions.TryParseStringToDouble(Profile.Name, out var value))
            {
                return;
            }
            
            var useGenAPI = value >= 18.00;
            API_URL = useGenAPI ? GEN_API_URL + $"?version={Profile.Name}" : GIT_ARCHIVE_URL;
        }
        
        var aes = await API.UEDB.Globals.API.GetAesAsync(API_URL, useBaseUrl: false);

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

        foreach (var key in aes.DynamicKeys)
        {
            if (!EncryptionKey.IsValidKey(key.Key))
            {
                continue;
            }

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
        
        if (value >= 15.20)
        {
            var mapping = await API.UEDB.Globals.API.FetchMappingAsync(Profile.Name);
        
            if (mapping is { LocalPath: not null })
            {
                Profile.MappingsContainer.Override = true;
                Profile.MappingsContainer.Path = mapping.LocalPath;
            }
        }
    }
}
