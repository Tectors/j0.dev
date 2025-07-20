using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using vj0.Application;
using vj0.Services;
using vj0.Services.Framework;
using vj0.Windows;

namespace vj0.Views;

public partial class TitleBar : UserControl
{
    public TitleBar()
    {
        InitializeComponent();
        
        AttachedToVisualTree += OnAttachedToVisualTree;
    }
    
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            captionButtons.Attach(window);
        }
    }
    
    private void EditProfile(object? sender, RoutedEventArgs e) => (VisualRoot as MainWindow)?.OnEditProfile(sender, e);

    private void RestartAPI(object? sender, RoutedEventArgs e)
    {
        AppServices.Cloud.Restart();
    }
    
    public void OpenGitHubLink(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink(Globals.GITHUB_LINK);
    }
}