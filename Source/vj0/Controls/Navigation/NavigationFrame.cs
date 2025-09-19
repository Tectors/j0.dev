using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

using vj0.Views;

namespace vj0.Controls.Navigation;

public partial class NavigationFrame : ContentControl
{
    public static readonly StyledProperty<bool> HideBorderProperty = AvaloniaProperty.Register<SettingsView, bool>(nameof(HideBorder));
    
    public static readonly StyledProperty<IBrush?> BackgroundOverrideProperty =
        AvaloniaProperty.Register<NavigationFrame, IBrush?>(nameof(BackgroundOverride));
    
    public static readonly StyledProperty<IBrush?> BorderOverrideProperty =
        AvaloniaProperty.Register<NavigationFrame, IBrush?>(nameof(BorderOverride));

    public static readonly StyledProperty<bool> DisableMarginProperty =
        AvaloniaProperty.Register<NavigationFrame, bool>(nameof(DisableMargin));
    
    public bool HideBorder
    {
        get => GetValue(HideBorderProperty);
        set => SetValue(HideBorderProperty, value);
    }
    
    public IBrush? BackgroundOverride
    {
        get => GetValue(BackgroundOverrideProperty);
        set => SetValue(BackgroundOverrideProperty, value);
    }
    
    public IBrush? BorderOverride
    {
        get => GetValue(BorderOverrideProperty);
        set => SetValue(BorderOverrideProperty, value);
    }

    public bool DisableMargin
    {
        get => GetValue(DisableMarginProperty);
        set => SetValue(DisableMarginProperty, value);
    }
    
    public NavigationFrame()
    {
        InitializeComponent();
    }
    
    private Border? _contentBorder;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentBorder = e.NameScope.Find<Border>("ContentBorder");

        if (_contentBorder is null)
        {
            return;
        }

        if (HideBorder)
        {
            _contentBorder.BorderThickness = new Thickness(0);
        }

        if (BackgroundOverride is not null)
        {
            _contentBorder.Background = BackgroundOverride;
        }
        else if (HideBorder)
        {
            _contentBorder.Background = Brushes.Transparent;
        }
        
        if (BorderOverride is not null)
        {
            _contentBorder.BorderBrush = BorderOverride;
        }

        _contentBorder.Margin = DisableMargin ? new Thickness(0) : new Thickness(0, 0, 12, 12);
    }
}
