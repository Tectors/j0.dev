using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using vj0.Windows;

namespace vj0.Views.Onboarding;

public partial class OnboardingTermsView : UserControl
{
    public OnboardingTermsView()
    {
        InitializeComponent();
        
        Navigation.OnboardingTerms.Initialize(NavigationView);
        
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ContinueButton.IsEnabled = true;
            });
        });
    }
    
    private void Next(object? sender, RoutedEventArgs e)
    {
        var window = VisualRoot as OnboardingWindow;
        window!.GoNext();
    }
}
