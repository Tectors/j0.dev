using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;
using vj0.Models;
using vj0.Models.Profiles;
using vj0.Shared.Framework.Base;
using vj0.Shared.Utilities;

namespace vj0.ViewModels.Profiles.Framework;

public partial class ProfileViewModelBase : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AutoDetectedGameToOpacity))]
    [NotifyPropertyChangedFor(nameof(LastUsed))]
    [NotifyPropertyChangedFor(nameof(IsUncompletedProfile))]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    private Profile? _profile;

    public virtual void UpdateProfileProperties()
    {
        OnPropertyChanged(nameof(LastUsed));
        OnPropertyChanged(nameof(IsUncompletedProfile));
        OnPropertyChanged(nameof(IsRunning));
    }

    protected ProfileViewModelBase()
    {
        Profile = new Profile();
    }
    
    /* Metadata ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    public double AutoDetectedGameToOpacity => Profile!.IsAutoDetected ? 0.5 : 1.0;
    public bool? IsRunning => Profile?.Status.State == EProfileStatus.Active;
    
    public string LastUsed => Profile!.Display.LastUsed.HasValue
        ? TimeUtilities.GetRelativeTime(Profile.Display.LastUsed.Value, RelativeTimeClock.Now)
        : "Never";
    
    public bool IsUncompletedProfile => Profile is null || Profile.Status.State == EProfileStatus.Uncompleted;
    
    /* Splash ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    public new virtual void Initialize()
    {
    }
}