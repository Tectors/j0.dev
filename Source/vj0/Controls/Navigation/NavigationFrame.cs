using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

using vj0.Views;

namespace vj0.Controls.Navigation;

public partial class NavigationFrame : ContentControl
{
    public static readonly StyledProperty<bool> HideBorderProperty = AvaloniaProperty.Register<SettingsView, bool>(nameof(HideBorder));
    
    public bool HideBorder
    {
        get => GetValue(HideBorderProperty);
        set => SetValue(HideBorderProperty, value);
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

        if (HideBorder && _contentBorder is not null)
        {
            _contentBorder.BorderThickness = new Thickness(0);
            _contentBorder.Background = Brushes.Transparent;
        }
    }
}
