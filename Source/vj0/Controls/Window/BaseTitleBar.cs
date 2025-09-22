using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;

namespace vj0.Controls.Window;

public partial class BaseTitleBar : UserControl
{
    public static readonly DirectProperty<BaseTitleBar, ObservableCollection<Control>> LeftItemsProperty =
        AvaloniaProperty.RegisterDirect<BaseTitleBar, ObservableCollection<Control>>(
            nameof(LeftItems),
            o => o.LeftItems,
            (o, v) => o.LeftItems = v);

    private ObservableCollection<Control> _leftItems = [];

    public ObservableCollection<Control> LeftItems
    {
        get => _leftItems;
        set => SetAndRaise(LeftItemsProperty, ref _leftItems, value);
    }

    public static readonly DirectProperty<BaseTitleBar, ObservableCollection<Control>> MiddleContentProperty =
        AvaloniaProperty.RegisterDirect<BaseTitleBar, ObservableCollection<Control>>(
            nameof(MiddleContent),
            o => o.MiddleContent,
            (o, v) => o.MiddleContent = v);

    private ObservableCollection<Control> _middleContent = [];
    public ObservableCollection<Control> MiddleContent
    {
        get => _middleContent;
        set => SetAndRaise(MiddleContentProperty, ref _middleContent, value);
    }

    public static readonly DirectProperty<BaseTitleBar, ObservableCollection<Control>> RightContentProperty =
        AvaloniaProperty.RegisterDirect<BaseTitleBar, ObservableCollection<Control>>(
            nameof(RightContent),
            o => o.RightContent,
            (o, v) => o.RightContent = v);

    private ObservableCollection<Control> _rightContent = [];
    public ObservableCollection<Control> RightContent
    {
        get => _rightContent;
        set => SetAndRaise(RightContentProperty, ref _rightContent, value);
    }
    
    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<BaseTitleBar, bool>(nameof(ShowIcon), true);

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public BaseTitleBar()
    {
        InitializeComponent();
        
        AttachedToVisualTree += OnAttachedToVisualTree;
    }
    
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is Avalonia.Controls.Window window)
        {
            captionButtons.Attach(window);
        }
    }
}