using Avalonia.Controls;
using Avalonia.Interactivity;

using vj0.Windows;

namespace vj0.Views.Onboarding;

public partial class OnboardingWelcomeView : UserControl
{
    public OnboardingWelcomeView()
    {
        InitializeComponent();
    }

    private void OnStartClicked(object? sender, RoutedEventArgs e)
    {
        var window = VisualRoot as OnboardingWindow;
        window!.GoNext();
    }
}