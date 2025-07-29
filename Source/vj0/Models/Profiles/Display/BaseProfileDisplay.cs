using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CUE4Parse.UE4.Objects.Core.Misc;
using vj0.Application;
using vj0.Shared.Framework.Base;

namespace vj0.Models.Profiles.Display;

public partial class BaseProfileDisplay : BaseProfile {
    [ObservableProperty] private ProfileDisplay _display;

    [JsonIgnore] protected bool EnableDisplayLinks = false;
    
    protected BaseProfileDisplay()
    {
        _display = new ProfileDisplay(this);
    }
    
    [JsonIgnore] protected string SavedFilePath => Path.Combine(Globals.ProfilesFolder.ToString(), FileName);

    public void Reload()
    {
        if (!EnableDisplayLinks) return;
        
        Display.Reload();
    }

    protected override void ArchiveDirectoryChanged()
    {
        if (!EnableDisplayLinks) return;
        
        Display.Profile = this;
        Display.Splash.Profile = this;
        
        Display.ReloadOnlySplash();
        OnPropertyChanged(nameof(Display));
    }
    
    protected override void NameChanged()
    {
        if (!EnableDisplayLinks) return;
        
        Display.Profile = this;
        Display.Splash.Profile = this;
        
        Display.ReloadOnlyAbbreviation();
    }

    /* Helper */
    public string FindContentFolder()
    {
        var index = ArchiveDirectory.IndexOf("Content", StringComparison.OrdinalIgnoreCase);
        if (index > 0) return ArchiveDirectory[..(index + "Content".Length)];
        
        var contentPath = Path.Combine(ArchiveDirectory, "Content");

        return Directory.Exists(contentPath) ? contentPath : ArchiveDirectory;
    }
    
    /* Used to display existing Pak Files */
    public List<BasePakFileEntry> PakFileEntries { get; set; } = [];

    protected List<BasePakFileEntry> GetPakFiles()
    {
        if (Provider is null)
        {
            return [];
        }

        var filter = new Regex(@"^(?!global|pakchunk.+(optional|ondemand)-).+\.(pak|utoc)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        var seenGuids = new HashSet<FGuid> { Globals.ZERO_GUID };
        var result = new List<BasePakFileEntry>();

        foreach (var vfs in Provider.UnloadedVfs)
        {
            if (!filter.IsMatch(vfs.Name))
            {
                continue;
            }

            if (vfs.EncryptionKeyGuid == Globals.ZERO_GUID || !seenGuids.Add(vfs.EncryptionKeyGuid))
            {
                continue;
            }

            result.Add(new BasePakFileEntry
            {
                FileName = vfs.Name,
                Guid = vfs.EncryptionKeyGuid.ToString()
            });
        }

        return result;
    }
}