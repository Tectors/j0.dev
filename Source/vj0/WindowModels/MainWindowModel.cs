using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using vj0.Application;
using vj0.Cloud.Controllers;
using vj0.Framework.Models;
using vj0.Models.Enums;
using vj0.Models.Profiles;
using vj0.Services;
using vj0.Services.Framework;
using vj0.Shared.Framework.Base;
using vj0.Views;
using vj0.Views.Profiles;

namespace vj0.WindowModels;

/* ~~~ Main Window ViewModel ~~~ */
public partial class MainWindowModel : WindowModelBase
{
    public static InfoService Info => AppServices.Info;
    
    public string Title => CurrentProfile is not null ? $"{CurrentProfile.Name} - {Globals.APP_NAME}" : $"{Globals.APP_NAME} ({Globals.VERSION})";

    /* ~~~ Observable State ~~~ */
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCurrentToolbarContentVisible))]
    private object? _currentToolbarContent;
    
    public bool IsCurrentToolbarContentVisible => CurrentToolbarContent is not null;

    [ObservableProperty]
    private AppStatus _status = AppStatus.Idle;
    
    [ObservableProperty]
    private string _textStatus = "";

    public void UpdateStatus(string status)
    {
        TextStatus = status;
    }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProfileDisplayName))]
    [NotifyPropertyChangedFor(nameof(DoesProfileExist))]
    [NotifyPropertyChangedFor(nameof(IsProfileInitialized))]
    [NotifyPropertyChangedFor(nameof(LoadedFilesDisplay))]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Profile? _currentProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProfileButtonOpacity))]
    private double _titleBarOpacity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleBarSecondGradientBrush))]
    private Color _titleBarSecondGradientColor = Colors.White;

    public Brush TitleBarSecondGradientBrush => new SolidColorBrush(TitleBarSecondGradientColor);

    [ObservableProperty]
    private LinearGradientBrush _titleBarGradientBrush = null!;

    /* ~~~ Profile Info ~~~ */
    public string ProfileDisplayName => CurrentProfile?.Name ?? "Unknown";
    public bool DoesProfileExist => CurrentProfile is not null;
    public bool IsProfileInitialized => CurrentProfile?.IsInitialized ?? false;
    
    public double ProfileButtonOpacity => Math.Max(TitleBarOpacity, 0.5);

    /* ~~~ Lifecycle Methods ~~~ */
    public new void Initialize()
    {
        if (CurrentProfile is not null)
        {
            CurrentProfile.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(Profile.Name))
                {
                    OnPropertyChanged(nameof(ProfileDisplayName));
                }
            };
        }
        
        NavigateToStatus(AppStatus.Idle);
        
        _ = ProfileSelectionVM.RefreshAllAsync();
    }

    /* ~~~ Profile Handling ~~~ */
    private void SetCurrentProfile(Profile? profile)
    {
        var lastProfile = CurrentProfile;
        CurrentProfile = profile;

        if (CurrentProfile is not null)
        {
            CloudApiController.SetProfile(CurrentProfile);
        }
        
        RefreshProfileProperties();

        lastProfile?.DisposeProvider();
    }

    public void RefreshProfileProperties()
    {
        if (CurrentProfile is null)
        {
            TitleBarOpacity = 0.0;
        }

        OnPropertyChanged(nameof(ProfileDisplayName));
        OnPropertyChanged(nameof(DoesProfileExist));
        OnPropertyChanged(nameof(IsProfileInitialized));

        UpdateGradientBrush();
    }

    public void OnNavigationItemSelected(Type pageType)
    {
        if (pageType == typeof(ProfileSelectionView))
        {
            CurrentToolbarContent = new ProfileSelectionViewToolbar();
        }
        else
        {
            CurrentToolbarContent = null;
        }
    }

    public void RequestEditProfile()
    {
        CurrentProfile?.OpenEditor();
    }
    
    public void NavigateToExplorer()
    {
        if (!Globals.IsReadyToExplore || !IsProfileInitialized) return;
        
        Navigation.App.Open(typeof(ExplorerView));
    }

    /* ~~~ Status Transitions ~~~ */
    public void NavigateToStatus(AppStatus newStatus)
    {
        var previousStatus = Status;
        Status = newStatus;

        switch (newStatus)
        {
            case AppStatus.Idle:
            {
                Discord.UpdateState("Idle");
                Discord.UpdateDetails("");
                
                if (previousStatus == AppStatus.Active)
                {
                    if (CurrentProfile is not null)
                    {
                        SetCurrentProfile(null);
                    }

                    StopTitleBarBeatEffect();
                }
            }
            break;

            case AppStatus.Active:
            {
                UpdateDiscordDetailStatus();
                Discord.UpdateState("Active");
                
                StartTitleBarBeatEffect();
            }
            break;

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null);
            }
        }
    }

    /* ~~~ Title Bar Animation ~~~ */
    private bool isBeating;
    private DispatcherTimer? beatTimer;
    private double beatTime;
    private double lastKnownOpacity = 1.0;
    
    private const double BeatIncrement = 0.1;
    private static readonly TimeSpan BeatInterval = TimeSpan.FromMilliseconds(7);

    private void StartTitleBarBeatEffect()
    {
        if (isBeating || Status == AppStatus.Idle)
        {
            return;
        }

        isBeating = true;
        beatTime = 0;
        TitleBarOpacity = 0.0;

        if (beatTimer is null)
        {
            beatTimer = new DispatcherTimer { Interval = BeatInterval };
            beatTimer.Tick += (_, _) =>
            {
                if (!isBeating)
                {
                    return;
                }

                beatTime += BeatIncrement;
                var t = (Math.Sin(beatTime) + 1) / 2;
                var eased = t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 4;
                var opacity = 0.0 + eased * 0.7;

                TitleBarOpacity = opacity;
                lastKnownOpacity = opacity;
            };
        }

        beatTimer.Start();
    }

    private void StopTitleBarBeatEffect()
    {
        beatTimer?.Stop();
        beatTimer = null;

        if (Status == AppStatus.Idle)
        {
            TitleBarOpacity = 0.0;
            isBeating = false;
            return;
        }

        if (!isBeating)
        {
            return;
        }

        isBeating = false;
        RefreshProfileProperties();

        var stopwatch = Stopwatch.StartNew();
        const double duration = 0.475;

        var fadeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        fadeTimer.Tick += (_, _) =>
        {
            var t = Math.Clamp(stopwatch.Elapsed.TotalSeconds / duration, 0, 1);
            var eased = t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
            var startOpacity = lastKnownOpacity;
            var newOpacity = startOpacity + (1.0 - startOpacity) * eased;

            TitleBarOpacity = newOpacity;

            if (t >= 1)
            {
                fadeTimer.Stop();
            }
        };

        fadeTimer.Start();
    }

    private void UpdateGradientBrush()
    {
        if (CurrentProfile is null)
        {
            TitleBarOpacity = 0.0;
            
            return;
        }

        var color = CurrentProfile?.Display.GradientBrush!.GradientStops[0].Color ?? Color.Parse("#fc0388");

        var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        if (luminance < 0.15)
        {
            color = Colors.White;
        }

        TitleBarSecondGradientColor = color;

        TitleBarGradientBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(100, 0, RelativeUnit.Absolute),
            EndPoint = new RelativePoint(350, 0, RelativeUnit.Absolute),
            GradientStops =
            [
                new GradientStop(Color.Parse("#0d0d0d"), 0.02),
                new GradientStop(color, 0.25),
                new GradientStop(color, 0.5),
                new GradientStop(Color.Parse("#0d0d0d"), 1.0)
            ]
        };

        OnPropertyChanged(nameof(MainWM.TitleBarGradientBrush));
        OnPropertyChanged(nameof(TitleBarSecondGradientBrush));
    }
    
    public static bool IsAPIServiceEnabled => Settings is not null && Settings.Cloud.RunHostedAPI;
    public static bool IsAPIServiceRunning => AppServices.Cloud.API is not null && AppServices.Cloud.API!.IsRunning;
    public static bool IsAPIServiceErrored => AppServices.Cloud.API is not null && AppServices.Cloud.API!.HasErrored;

    public void UpdateAPIServiceEnabled()
    {
        OnPropertyChanged(nameof(IsAPIServiceEnabled));
    }
    
    public void UpdateAPIServiceStatusColor()
    {
        OnPropertyChanged(nameof(IsAPIServiceRunning));
        OnPropertyChanged(nameof(IsAPIServiceErrored));
    }
    
    private CancellationTokenSource? LastProfileCancellationTokenSource = new();
    
    public async Task StartProfileAsync(Profile profile)
    {
        if (CurrentProfile is not null)
        {
            CurrentProfile.DisposeProvider();
            
            await Dispatcher.UIThread.InvokeAsync(() => ProfileSelectionVM.UpdateProfileCard(CurrentProfile));
        }

        if (profile is null)
        {
            return;
        }
        
        LastProfileCancellationTokenSource?.Cancel();
        LastProfileCancellationTokenSource?.Dispose();
        
        LastProfileCancellationTokenSource = new CancellationTokenSource();
        
        SetCurrentProfile(profile);
        RefreshProfileProperties();

        if (CurrentProfile is null)
        {
            return;
        }
        
        CurrentProfile.Display.LastUsed = DateTime.Now;
        _ = CurrentProfile.Save();

        CurrentProfile.CheckStatusNotifies();
        CurrentProfile.Status.SetState(EProfileStatus.Active);
        CurrentProfile.IsInitialized = false;

        _ = Dispatcher.UIThread.InvokeAsync(() => ProfileSelectionVM.UpdateProfileCard(CurrentProfile));

        NavigateToStatus(AppStatus.Active);
        
        CurrentProfile.ResetEvents();
        CurrentProfile.OnInitialized += OnProfileInitialized;
        CurrentProfile.OnInitializationFailure += OnProfileInitializationFailure;

        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            await CurrentProfile.Initialize(LastProfileCancellationTokenSource!.Token);
        });
    }

    private void OnProfileInitialized(Profile inProfile)
    {
        if (inProfile.FileName != CurrentProfile!.FileName) return;
        
        Dispatcher.UIThread.InvokeAsync(StopTitleBarBeatEffect);
        Dispatcher.UIThread.InvokeAsync(() => ProfileSelectionVM.UpdateProfileCard(CurrentProfile));

        UpdateDiscordDetailStatus();
        OnPropertyChanged(nameof(IsProfileInitialized));
    }
    
    private async void OnProfileInitializationFailure(Profile inProfile)
    {
        if (inProfile.FileName != CurrentProfile!.FileName) return;
        
        NavigateToStatus(AppStatus.Idle);
                    
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProfileSelectionVM.UpdateProfileCard(inProfile);
            
            var dialog = new ContentDialog
            {
                Title = $"{inProfile.Name} failed to load",
                Content = inProfile.Status.FailureReason,
                CloseButtonText = "Dismiss",
                PrimaryButtonText = "Edit Profile",
                PrimaryButtonCommand = new RelayCommand(() =>
                {
                    inProfile.OpenEditor();
                })
            };
            
            _ = dialog.ShowAsync();
        });
    }

    private void UpdateDiscordDetailStatus()
    {
        var details = IsProfileInitialized
            ? $"{CurrentProfile!.Name} {LoadedFilesDisplay}"
            : $"{CurrentProfile!.Name} — Idling";

        Discord.UpdateDetails(details);
    }
    
    public static string LoadedFilesDisplay
    {
        get
        {
            var count = ExplorerVM.AssetCount;
            var formattedNumber = FormatNumber(count);
            
            return $"— {formattedNumber} assets";
        }
    }
    
    public void UpdateLoadedFilesDisplay()
    {
        OnPropertyChanged(nameof(LoadedFilesDisplay));
    }
    
    private static string FormatNumber(long number)
    {
        return number switch
        {
            >= 1_000_000_000 => $"{number / 1_000_000_000.0:F1} billion",
            >= 1_000_000 => $"{number / 1_000_000.0:F1} million",
            _ => number.ToString("N0")
        };
    }
}