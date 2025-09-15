using Avalonia.Interactivity;

using vj0.Framework.Models;
using vj0.WindowModels;

namespace vj0.Windows;

public partial class MainWindow : WindowBase<MainWindowModel>
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = WindowModel;
        
        Navigation.App.Initialize(MainNavigationView);
        Navigation.App.OnNavigate += WindowModel.OnNavigationItemSelected;

        WindowModel.Initialize();
    }

    public void OnEditProfile(object? sender, RoutedEventArgs e)
    {  
        WindowModel.RequestEditProfile();
    }
}
