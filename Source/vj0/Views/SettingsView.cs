using Avalonia;
using Avalonia.Controls;
using vj0.Services;

namespace vj0.Views;

public partial class SettingsView : UserControl
{
    public static readonly StyledProperty<bool> IsOnboardingProperty = AvaloniaProperty.Register<SettingsView, bool>(nameof(IsOnboarding));
    
    public bool IsOnboarding
    {
        get => GetValue(IsOnboardingProperty);
        set => SetValue(IsOnboardingProperty, value);
    }

    private NavigatorContext Context = Navigation.Settings;
    private bool HasInitialized;
    
    public SettingsView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) =>
        {
            if (HasInitialized) return;
            
            if (IsOnboarding)
            {
                Context = new NavigatorContext();
            }
                
            Context.Initialize(NavigationView);
                
            HasInitialized = true;
        };
    }
}