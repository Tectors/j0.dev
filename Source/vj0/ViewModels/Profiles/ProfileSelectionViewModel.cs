using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Application;
using vj0.Framework.Models;
using vj0.Models;
using vj0.Models.Profiles;
using vj0.Services.Framework;
using vj0.Views.Profiles;

namespace vj0.ViewModels.Profiles;

public partial class ProfileSelectionViewModel : ViewModelBase
{
    public Dictionary<string, ProfileCard> CardMap { get; } = new();
    private Dictionary<string, ProfileCardViewModel> ViewModelMap { get; } = new();

    [ObservableProperty] private bool isEmpty;
    
    public Panel? ProfileListPanel { get; set; }
    public Func<ProfileCard, Border>? WrapCard { get; set; }
    public Action<ProfileCard>? HookEvents { get; set; }
    
    private bool hasDetectedGames;

    private async Task LoadAll()
    {
        if (!hasDetectedGames)
        {
            hasDetectedGames = true;
        
            await GameDetection.LoadAllAsync();
            await GameDetection.DetectAllProfilesAsync();
        }
    }
    
    private bool hasAttemptedRecentProfileLoad;
    
    public async Task RefreshAllAsync()
    {
        await LoadAll();
        
        if (AppServices.Settings.Application.LoadRecentProfileOnLaunch && MainWM.CurrentProfile is null && !hasAttemptedRecentProfileLoad)
        {
            var recentProfile = GameDetection.GetRecentlyUsedProfiles(1).FirstOrDefault() ?? GameDetection.LoadedProfiles.FirstOrDefault();

            if (recentProfile is not null)
            {
                _ = MainWM.StartProfileAsync(recentProfile);
            }

            hasAttemptedRecentProfileLoad = true;
        }

        CardMap.Clear();
        ViewModelMap.Clear();

        var profiles = GameDetection.LoadedProfiles;

        var sorted = profiles.OrderByDescending(p => !GetSortKey(p).IsNumeric)
           .ThenByDescending(p => GetSortKey(p).NumericVersion)
           .ThenBy(p => GetSortKey(p).NameLower)
           .ToList();

        _ = Task.Run(() =>
        {
            foreach (var profile in sorted)
            {
                profile.TryAutoFetchAesKeys();
            }
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (ProfileListPanel is not null)
            {
                ProfileListPanel.Children.Clear();
            }

            foreach (var profile in sorted)
            {
                var card = CreateCard(profile);
                CardMap[profile.FileName] = card;
                
                if (ProfileListPanel is null) continue;
                
                ProfileListPanel.Children.Add(WrapCard!(card));
            }
        });
        
        IsEmpty = CardMap.Count == 0;
    }

    public void AddProfile(Profile profile)
    {
        if (CardMap.ContainsKey(profile.ArchiveDirectory))
        {
            return;
        }

        var card = CreateCard(profile);
        CardMap[profile.FileName] = card;
            
        IsEmpty = CardMap.Count == 0;

        InsertCardSorted(card);
    }

    public void UpdateProfileCard(Profile profile)
    {
        if (profile is null || profile.FileName is null) return;
        
        if (ProfileListPanel is null)
        {
            if (CardMap.TryGetValue(profile.FileName, out var savedCard))
            {
                savedCard.ViewModel.UpdateProfileProperties();
            }

            return;
        }

        if (CardMap.TryGetValue(profile.FileName, out var card))
        {
            card.ViewModel.UpdateProfileProperties();

            var existingBorder = ProfileListPanel.Children
                .OfType<Border>()
                .FirstOrDefault(b => b.Child == card);

            if (existingBorder is null) return;

            ProfileListPanel.Children.Remove(existingBorder);
            IsEmpty = CardMap.Count == 0;

            var newKey = GetSortKey(profile);

            for (var i = 0; i < ProfileListPanel.Children.Count; i++)
            {
                if (ProfileListPanel.Children[i] is not Border border || border.Child is not ProfileCard existingCard ||
                    existingCard.ViewModel.Profile is null)
                {
                    continue;
                }

                var existingKey = GetSortKey(existingCard.ViewModel.Profile);

                var compare = newKey.IsNumeric switch
                {
                    false when !existingKey.IsNumeric => string.Compare(newKey.NameLower, existingKey.NameLower, StringComparison.Ordinal),
                    true when existingKey.IsNumeric => existingKey.NumericVersion!.Value.CompareTo(newKey.NumericVersion!.Value),
                    _ => newKey.IsNumeric ? 1 : -1
                };

                if (compare >= 0) continue;

                ProfileListPanel.Children.Insert(i, existingBorder);
                return;
            }

            ProfileListPanel.Children.Add(existingBorder);
        }
    }

    private void InsertCardSorted(ProfileCard card)
    {
        if (card.ViewModel.Profile is null || ProfileListPanel is null) return;

        var newKey = GetSortKey(card.ViewModel.Profile);

        for (var i = 0; i < ProfileListPanel.Children.Count; i++)
        {
            if (ProfileListPanel.Children[i] is not Border border || border.Child is not ProfileCard existingCard ||
                existingCard.ViewModel.Profile is null)
            {
                continue;
            }

            var existingKey = GetSortKey(existingCard.ViewModel.Profile);

            var compare = newKey.IsNumeric switch
            {
                false when !existingKey.IsNumeric => string.Compare(newKey.NameLower, existingKey.NameLower, StringComparison.Ordinal),
                true when existingKey.IsNumeric => existingKey.NumericVersion!.Value.CompareTo(newKey.NumericVersion!.Value),
                _ => newKey.IsNumeric ? 1 : -1
            };

            if (compare >= 0) continue;

            ProfileListPanel.Children.Insert(i, WrapCard!(card));
            return;
        }

        ProfileListPanel.Children.Add(WrapCard!(card));
    }

    private ProfileCard CreateCard(Profile profile)
    {
        ProfileCardViewModel vm;
        
        if (Globals.HideAllProfileCardInformation)
        {
            var newProfile = profile.LazyClone();
            newProfile.Display.SetRandomGradient();
            newProfile.ArchiveDirectory = @"D:\Builds\Tropical\Game\Content\Paks";
            newProfile.Name = "Game";
            
            vm = GetOrCreateProfileViewModel(newProfile);
        }
        else
        {
            vm = GetOrCreateProfileViewModel(profile);
        }
        
        var card = new ProfileCard(vm);
        
        HookEvents?.Invoke(card);
        return card;
    }

    private ProfileCardViewModel GetOrCreateProfileViewModel(Profile profile)
    {
        if (ViewModelMap.TryGetValue(profile.FileName, out var existing))
        {
            return existing;
        }

        var newVm = new ProfileCardViewModel { Profile = profile };
        newVm.Initialize();
        
        ViewModelMap[profile.FileName] = newVm;
        
        return newVm;
    }

    private static (bool IsNumeric, decimal? NumericVersion, string NameLower) GetSortKey(Profile profile)
    {
        if (decimal.TryParse(profile.Name, NumberStyles.Number, CultureInfo.InvariantCulture, out var version))
        {
            return (true, version, string.Empty);
        }

        return (false, null, profile.Name.ToLowerInvariant());
    }
}