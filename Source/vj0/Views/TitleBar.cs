using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

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
        AppService.OpenLink($"{Globals.GITHUB_COMMIT_LINK}/{Globals.COMMIT}");
    }
    
    public void OpenGitHubLicense(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink($"{Globals.GITHUB_LINK}/blob/main/LICENSE");
    }

    private void CopyGitCloneCommand(object? sender, RoutedEventArgs e)
    {
        if (Globals.IS_COMMIT_AVAILABLE)
        {
            App.CopyText($"git clone --recurse-submodules {Globals.GITHUB_LINK}.git && cd {Globals.GITHUB_REPO_NAME} && git checkout {Globals.COMMIT} && git submodule update --init --recursive\n");
        }
        else
        {
            App.CopyText($"git clone --recurse-submodules {Globals.GITHUB_LINK}.git && cd {Globals.GITHUB_REPO_NAME} && git submodule update --init --recursive\n");
        }
    }
}