using Avalonia.Controls;
using Avalonia.Interactivity;
using vj0.Windows;

namespace vj0.Views.Onboarding;

public partial class OnboardingTermsView : UserControl
{
    public OnboardingTermsView()
    {
        InitializeComponent();
        
        Navigation.OnboardingTerms.Initialize(NavigationView);
    }

    private void Next(object? sender, RoutedEventArgs e)
    {
        var window = VisualRoot as OnboardingWindow;
        window!.GoNext();
    }
}