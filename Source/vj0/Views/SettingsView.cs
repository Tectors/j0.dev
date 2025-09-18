using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
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
        
        NavigationView.TemplateApplied += (_, __) => FindPaneContentGrid();
        NavigationView.LayoutUpdated += (_, __) => UpdatePaneBorder();
    }
    
    private ItemsRepeater? NavigationLeftPanelContents;
    
    private void FindPaneContentGrid()
    {
        NavigationLeftPanelContents = NavigationView.GetVisualDescendants()
            .OfType<ItemsRepeater>()
            .FirstOrDefault(v => v.Name == "MenuItemsHost");

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

        PaneBorder.Height = NavigationLeftPanelContents.DesiredSize.Height + 26;
    }
}