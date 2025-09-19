using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

using vj0.Framework.Models;
using vj0.Services;
using vj0.ViewModels;

namespace vj0.Views;

public partial class SettingsView : ViewBase<SettingsViewModel>
{
    public static readonly StyledProperty<bool> IsOnboardingProperty = AvaloniaProperty.Register<SettingsView, bool>(nameof(IsOnboarding));
    
    public bool IsOnboarding
    {
        get => GetValue(IsOnboardingProperty);
        set => SetValue(IsOnboardingProperty, value);
    }

    private NavigatorContext Context = Navigation.Settings;
    private bool HasInitialized;
    
    public SettingsView(): base(SettingsVM)
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
        
        MainWM.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWM.CurrentProfile))
            {
                UpdateProfile();
            }
        };
        
        NavigationView.TemplateApplied += (_, __) => FindPaneContentGrid();
        NavigationView.LayoutUpdated += (_, __) => UpdatePaneBorder();
        
        UpdateProfile();
    }
    
    private void UpdateProfile()
    {
        SettingsVM.CurrentProfile = MainWM.CurrentProfile;

        var isCurrentProfileValid = SettingsVM.CurrentProfile is not null;
        
        NavigationView.Classes.Set("has-profile", isCurrentProfileValid);
        PaneBorder.Classes.Set("has-profile", isCurrentProfileValid);
        
        EditProfileButton.IsVisible = isCurrentProfileValid;
    }
    
    private ScrollViewer? NavigationLeftPanelContents;
    
    private void FindPaneContentGrid()
    {
        NavigationLeftPanelContents = NavigationView.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault(v => v.Name == "MenuItemsScrollViewer");

        if (NavigationLeftPanelContents is null)
        {
            Dispatcher.UIThread.Post(FindPaneContentGrid, DispatcherPriority.Render);
            return;
        }

        NavigationLeftPanelContents.SizeChanged += (_, __) => UpdatePaneBorder();
        
        UpdatePaneBorder();
    }
    private void UpdatePaneBorder()
    {
        if (NavigationLeftPanelContents is null)
        {
            return;
        }

        PaneBorder.Height = NavigationLeftPanelContents.DesiredSize.Height - PaneBorder.Margin.Top;
    }

    private void EditProfile(object? sender, RoutedEventArgs e)
    {
        MainWM.CurrentProfile!.OpenEditor();
    }
}