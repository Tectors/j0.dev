using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

using CommunityToolkit.Mvvm.ComponentModel;

using CUE4Parse.UE4.Versions;

using vj0.Converters.Enum;
using vj0.Models.Profiles.Paks;
using vj0.Core.Framework.Base;
using vj0.ViewModels.Profiles.Framework;

namespace vj0.ViewModels.ProfileEditor;

public partial class ProfileEditorViewModel : ProfileViewModelBase
{
    /* ~~~ Game Version Options ~~~ */
    public ObservableCollection<string> GameVersionOptions { get; } = new(
        Enum.GetNames(typeof(EGame))
            .Where(name => name.StartsWith("GAME_"))
            .Select(name => name[5..])
            .GroupBy(trimmed => Regex.IsMatch(trimmed, @"^UE\d+_") ? 1 : 0)
            .OrderBy(g => g.Key)
            .SelectMany(g => g)
    );
    
    [ObservableProperty]
    private string? _selectedVersionName;
   
    /* ~~~ Collections ~~~ */
    public ObservableCollection<PakKeyEntry> PakKeyEntries { get; set; } = [];
    
    public override void Initialize()
    {
        base.Initialize();
        
        if (Profile is null) return;
        
        Profile.Validate();

        SelectedVersionName = Profile.Version.ToString()[5..];

        if (Profile.Status.State == EProfileStatus.Uncompleted &&
            SelectedVersionName == EGameNameConverter.ToName(EGame.GAME_UE5_6))
        {
            SelectedVersionName = EGameNameConverter.ToName(EGame.GAME_UE5_7);
        }

        GeneratePakFileEntries();
    }

    public void GeneratePakFileEntries()
    {
        if (Profile is null) return;
        
        PakKeyEntries.Clear();
        
        foreach (var pakFileEntry in Profile.PakFileEntries)
        {
            var key = "";

            var matchingKey = Profile.Encryption.Keys.FirstOrDefault(k => k.Name == pakFileEntry.FileName);
            if (matchingKey is not null)
            {
                key = matchingKey.Key;
            }

            PakKeyEntries.Add(new PakKeyEntry
            {
                FileName = pakFileEntry.FileName,
                Key = key,
                Guid = pakFileEntry.Guid
            });
        }

        foreach (var aes in Profile.Encryption.Keys.Where(aes => PakKeyEntries.All(e => e.FileName != aes.Name)))
        {
            PakKeyEntries.Add(new PakKeyEntry
            {
                FileName = aes.Name,
                Key = aes.Key,
                Guid = aes.Guid
            });
        }

        var sorted = PakKeyEntries.OrderBy(e => e.FileName, StringComparer.OrdinalIgnoreCase).ToList();

        PakKeyEntries.Clear();

        foreach (var entry in sorted)
        {
            PakKeyEntries.Add(entry);
        }
    }
}
