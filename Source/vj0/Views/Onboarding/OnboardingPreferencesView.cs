using Avalonia.Controls;
using Avalonia.Interactivity;
using vj0.Windows;

namespace vj0.Views.Onboarding;

public partial class OnboardingPreferencesView : UserControl
{
    public OnboardingPreferencesView()
    {
        InitializeComponent();
    }

    private void Save(object? sender, RoutedEventArgs e)
    {
        var window = VisualRoot as OnboardingWindow;
        window!.GoNext();
    }
}