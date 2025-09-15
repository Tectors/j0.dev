using System;
using System.IO;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.IdentityModel.Tokens;

using vj0.Converters.Enum;
using vj0.Models.Profiles;
using vj0.Core.Framework.Base;
using vj0.Core.Framework.CUEParse;
using vj0.Plugins.Resolvers;
using vj0.ViewModels.ProfileEditor;

namespace vj0.WindowModels;

/* ~~~ ProfileEditorWindowModel ~~~ */
public partial class ProfileEditorWindowModel : ProfileEditorViewModel
{
    /* ~~~ State ~~~ */
    public Profile OriginalProfile { get; set; } = null!;

    public void CloseWindow()
    {
        OnClose?.Invoke();
    }

    [ObservableProperty]
    public bool _hasArchiveResolver = false;

    /* ~~~ Observable Properties ~~~ */
    [ObservableProperty] 
    private string? _titleBarText = "New Profile";
    
    /* ~~~ Computed Properties ~~~ */
    public object SaveChangesText => 
        Profile?.Status.State == EProfileStatus.Uncompleted ? "Create" : 
        Profile?.Status.State == EProfileStatus.Active ? "Save and Load" : 
        "Save Changes";

    public bool? ShowEncryptionTab => Profile?.PakFileEntries.Count > 0;

    /* ~~~ Constructor ~~~ */
    public ProfileEditorWindowModel()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Profile))
            {
                OnProfileChanged(); }
        };
        
    }
    
    /* ~~~ Events ~~~ */
    public event Action<Profile, Profile, bool>? ProfileSaved;
    public event Action? OnClose;

    public void Reset()
    {
        ProfileSaved = null;
        PakKeyEntries = [];
    }

    /* ~~~ Initialization ~~~ */
    public override void Initialize()
    {
        base.Initialize();
        
        Profile!.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Profile.ArchiveDirectory))
            {
                OnArchiveDirectoryChanged();
            }
        };
    }

    private void OnArchiveDirectoryChanged()
    {
        if (Profile is null ||
            /* This operation is to fill the name if it is empty using the archive directory */
            !Profile.Name.IsNullOrEmpty()
            || Profile.ArchiveDirectory.IsNullOrEmpty()
            || !Directory.Exists(Profile.ArchiveDirectory)) return;
        
        var parts = Profile.ArchiveDirectory.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);

        string? version = null;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!parts[i].Equals("Content", StringComparison.OrdinalIgnoreCase) || !parts[i + 1].Equals("Paks", StringComparison.OrdinalIgnoreCase)) continue;
            
            if (i >= 2)
            {
                version = parts[i - 2];
            }
            
            break;
        }

        if (version is not null)
        {
            Profile.Name = version;

            Profile.Version = Profile.PredictBaseUEVersion(Profile.Name);
            SelectedVersionName = Profile.Version.ToString()[5..];
        }
    }

    private void OnProfileChanged()
    {
        OnPropertyChanged(nameof(SaveChangesText));
        OnPropertyChanged(nameof(ShowEncryptionTab));
        
        TitleBarText = Profile!.IsNameEmpty ? "New Profile" : $"Editing {Profile.Name}";
        
        Profile.ResolvePluginHandler();
        HasArchiveResolver = Profile!.Plugins.Any(p => p is IArchiveResolverPlugin);
    }

    public void Save()
    {
        if (Profile is null || Profile.HasErrors || SelectedVersionName is null)
        {
            return;
        }
        
        if (EGameNameConverter.TryParse(SelectedVersionName, out var selectedGame))
        {
            Profile.Version = selectedGame;
        }

        Profile.Encryption.Keys = PakKeyEntries
            .Where(entry =>
                !string.IsNullOrWhiteSpace(entry.Key) ||
                Profile.Encryption.Keys.Any(k =>
                    k.Name == entry.FileName &&
                    !string.IsNullOrWhiteSpace(k.Key) &&
                    k.Key != entry.Key))
            .GroupBy(entry => entry.FileName)
            .Select(group => group.First())
            .Select(entry => new EncryptionKey
            {
                Name = entry.FileName,
                Key = entry.Key,
                Guid = entry.Guid
            }).ToList();

        ProfileSaved?.Invoke(OriginalProfile, Profile, IsUncompletedProfile);
    }
}
