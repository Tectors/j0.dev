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
        AppService.OpenLink($"{GITHUB_COMMIT_LINK}/{COMMIT}");
    }
    
    public void OpenGitHubLicense(object? sender, RoutedEventArgs e)
    {
        AppService.OpenLink($"{GITHUB_LINK}/blob/main/LICENSE");
    }

    private void CopyGitCloneCommand(object? sender, RoutedEventArgs e)
    {
        App.CopyText(IS_COMMIT_AVAILABLE
            ? $"git clone --recurse-submodules {GITHUB_LINK}.git && cd {GITHUB_REPO_NAME} && git checkout {COMMIT} && git submodule update --init --recursive\n"
            : $"git clone --recurse-submodules {GITHUB_LINK}.git && cd {GITHUB_REPO_NAME} && git submodule update --init --recursive\n");
    }
}
