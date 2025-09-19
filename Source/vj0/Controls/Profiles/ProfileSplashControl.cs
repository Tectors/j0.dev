using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace vj0.Controls.Profiles;

public partial class ProfileSplashControl : UserControl
{
    /* 30 default width and height */
    private static readonly StyledProperty<double> SplashScaleProperty = AvaloniaProperty.Register<ProfileSplashControl, double>(nameof(SplashScale), 1.0);

    public double SplashScale
    {
        get => GetValue(SplashScaleProperty);
        set => SetValue(SplashScaleProperty, value);
    }
    
    private static readonly StyledProperty<CornerRadius> RadiusProperty = AvaloniaProperty.Register<ProfileSplashControl, CornerRadius>(nameof(Radius), new CornerRadius(12));

    public CornerRadius Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }
        
    private static readonly StyledProperty<Thickness> OutlineBorderThicknessProperty = AvaloniaProperty.Register<ProfileSplashControl, Thickness>(nameof(OutlineBorderThickness), new Thickness(0));

    public Thickness OutlineBorderThickness
    {
        get => GetValue(OutlineBorderThicknessProperty);
        set => SetValue(OutlineBorderThicknessProperty, value);
    }
    
    public static readonly StyledProperty<IBrush?> OutlineBorderBrushProperty =
        AvaloniaProperty.Register<ProfileSplashControl, IBrush?>(nameof(OutlineBorderBrush), Brushes.White);
    
    public IBrush? OutlineBorderBrush
    {
        get => GetValue(OutlineBorderBrushProperty);
        set => SetValue(OutlineBorderBrushProperty, value);
    }

    private static readonly StyledProperty<bool> IgnoreLoadingProperty = AvaloniaProperty.Register<ProfileSplashControl, bool>(nameof(IgnoreLoading), false);

    public bool IgnoreLoading
    {
        get => GetValue(IgnoreLoadingProperty);
        set => SetValue(IgnoreLoadingProperty, value);
    }

    public ProfileSplashControl()
    {
        InitializeComponent();
    }
    
    public ProfileSplashControl(float splashScale)
    {
        InitializeComponent();
        
        SplashScale = splashScale;
    }
}
