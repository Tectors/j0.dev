using Avalonia.Controls;
using Avalonia.Interactivity;

using vj0.Application;
using vj0.Framework.Models;
using vj0.Services;
using vj0.Views.Onboarding;
using vj0.WindowModels;

namespace vj0.Windows;

public partial class OnboardingWindow : WindowBase<OnboardingWindowModel>
{
    public OnboardingWindow()
    {
        InitializeComponent();
        captionButtons.Attach((VisualRoot as Window)!);
        
        DataContext = WindowModel;

        WindowModel.CurrentPageIndex = 0;
        
        Navigation.Onboarding = new NavigatorContext();
        Navigation.Onboarding.Initialize(OnboardingNavigationView);
        Navigation.Onboarding.Open(typeof(OnboardingWelcomeView));
    }
    
    private void Back(object? sender, RoutedEventArgs e)
    {
        if (!WindowModel.CanGoBack) return;

        WindowModel.GoBack();
        
        var type = WindowModel.CurrentPageType;
        Navigation.Onboarding.Open(type);
    }

    private void Next(object? sender, RoutedEventArgs e)
    {
        GoNext();
    }
    
    public void GoNext()
    {
        if (!WindowModel.CanGoNext) return;

        WindowModel.GoNext();
        
        var type = WindowModel.CurrentPageType;

        if (type == typeof(MainWindow))
        {
            var app = (AppInstance)Avalonia.Application.Current!;
            
            if (app.savedWindow.GetType() != typeof(MainWindow))
            {
                app.SpawnWindow(new MainWindow());
            }

            Close();
        
            Settings.Application.CompletedOnboarding = true;
            Settings.Save();
            
            return;
        }
        
        Navigation.Onboarding.Open(type);
    }
        
    private void OpenDonateLink(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(DONATE_LINK);
    }
    
    private void OpenDiscordLink(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(DISCORD_LINK);
    }
}
