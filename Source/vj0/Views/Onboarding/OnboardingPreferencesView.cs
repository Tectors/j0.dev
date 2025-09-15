using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using vj0.Windows;

namespace vj0.Views.Onboarding;

public partial class OnboardingPreferencesView : UserControl
{
    public OnboardingPreferencesView()
    {
        InitializeComponent();
        
        Task.Run(async () =>
        {
            await Task.Delay(500);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ContinueButton.IsEnabled = true;
            });
        });
    }

    private void Save(object? sender, RoutedEventArgs e)
    {
        var window = VisualRoot as OnboardingWindow;
        window!.GoNext();
    }
}
