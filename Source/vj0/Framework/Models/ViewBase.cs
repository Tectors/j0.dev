using Avalonia.Controls;
using Avalonia.Interactivity;

using Microsoft.Extensions.DependencyInjection;

using vj0.Services.Framework;

namespace vj0.Framework.Models;

public abstract class ViewBase<T> : UserControl where T : ViewModelBase
{
    protected readonly T ViewModel;

    protected ViewBase(T? templateViewModel = null, bool initializeModel = true)
    {
        ViewModel = templateViewModel ?? AppServices.Services.GetRequiredService<T>();
        DataContext = ViewModel;

        if (initializeModel)
        {
            Tasks.Run(ViewModel.Initialize);
        }
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        await ViewModel.OnViewOpened();
    }

    protected override async void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        
        await ViewModel.OnViewExited();
    }
}