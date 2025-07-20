using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Serilog;
using vj0.Framework.Models;
using vj0.Models;
using vj0.Models.Profiles;
using vj0.Services;
using vj0.Shared.Framework.Base;
using vj0.WindowModels;

namespace vj0.Windows;

/* ~~~ ProfileEditorWindow ~~~ */
public partial class ProfileEditorWindow : WindowBase<ProfileEditorWindowModel>
{
    private readonly NavigatorContext NavigatorContext = new();
    
    public ProfileEditorWindow()
    {
        var newProfile = new Profile
        {
            Status =
            {
                State = EProfileStatus.Uncompleted
            }
        };

        WindowModel.Profile = newProfile;
        WindowModel.OriginalProfile = newProfile;

        Setup();
    }

    public ProfileEditorWindow(Profile existingProfile)
    {
        WindowModel = new ProfileEditorWindowModel
        {
            Profile = existingProfile.Clone(),
            OriginalProfile = existingProfile
        };

        Setup();
    }
    
    /* ~~~ Setup Logic ~~~ */
    private void Setup()
    {
        WindowModel.Reset();
        
        InitializeComponent();
        captionButtons.Attach((VisualRoot as Window)!);

        NavigatorContext.Initialize(ProfileEditorNavigationView);

        WindowModel.Initialize();
        WindowModel.ProfileSaved += OnProfileSaved;
        WindowModel.OnClose += Close;
        
        DataContext = WindowModel;
    }
    
    private async void OnProfileSaved(Profile originalProfile, Profile newProfile, bool isUncompleted)
    {
        if (newProfile == null || originalProfile == null)
        {
            return;
        }

        var hasChanged = !originalProfile.Compare(newProfile);
        var hasVisuallyChanged = !string.Equals(originalProfile.Name, newProfile.Name, StringComparison.Ordinal) || hasChanged;

        if (originalProfile.Status.State == EProfileStatus.Uncompleted)
        {
            await FetchUndetectedMetadataAsync();
        }
        else if (!hasChanged)
        {
            Log.Information($"Profile {newProfile.Name} was deemed unchanged");
        }
        
        /* We still save it, because name changes can occur. */
        SaveProfileToDisk(originalProfile, newProfile);

        UpdateMainWindowProfileState(originalProfile, isUncompleted, hasChanged, hasVisuallyChanged);

        Close();
    }

    private static void SaveProfileToDisk(Profile originalProfile, Profile newProfile)
    {
        originalProfile.CopyFrom(newProfile);
        _ = originalProfile.Save();
        
        Log.Information($"Saved Profile {newProfile.Name}");
        
        Info.Message($"Saved {newProfile.Name}", "", InfoBarSeverity.Success, closeTime: 0.75f);
    }

    private void UpdateMainWindowProfileState(Profile originalProfile, bool isUncompleted, bool hasChanged, bool hasVisuallyChanged)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (desktop.MainWindow is not MainWindow mainWindow || mainWindow.DataContext is not MainWindowModel mainVM)
        {
            return;
        }

        originalProfile.Reload();

        if (isUncompleted)
        {
            originalProfile.CheckStatusNotifies();
            originalProfile.Status.SetState(EProfileStatus.Idle);
            
            ProfileSelectionVM.AddProfile(originalProfile);
            GameDetection.LoadedProfiles.Add(originalProfile);
        }
        else
        {
            if (hasVisuallyChanged)
            {
                ProfileSelectionVM.UpdateProfileCard(originalProfile);
            }
        }

        if (originalProfile.Status.State == EProfileStatus.Active)
        {
            mainVM.RefreshProfileProperties();

            if (hasChanged)
            {
                _ = mainVM.StartProfileAsync(originalProfile);
            }
        }
    }

    /* ~~~ UI Event Handlers ~~~ */
    public void Save(object? sender, RoutedEventArgs e)
    {
        if (WindowModel is null || WindowModel.Profile is null)
        {
            return;
        }

        if (WindowModel.Profile.HasErrors)
        {
            return;
        }

        WindowModel.Save();
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async Task FetchUndetectedMetadataAsync()
    {
        await WindowModel.Profile!.TryAutoFetchAesKeysUndetected();
        
        WindowModel.GeneratePakFileEntries();
    }

    private async void Button_FetchFromDataBase(object? sender, RoutedEventArgs e)
    {
        await FetchUndetectedMetadataAsync();
    }
}