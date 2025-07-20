using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using vj0.Services.Framework;

namespace vj0.Framework.Models;

public abstract class WindowBase<T> : Window where T : ViewModelBase
{
    protected T WindowModel;

    protected WindowBase(T? templateWindowModel = null, bool initializeModel = true)
    {
        WindowModel = templateWindowModel ?? AppServices.Services.GetService<T>()!;
        
        if (WindowModel is WindowModelBase wm)
        {
            wm.Window = this;
        }
        
        if (initializeModel)
        {
            Tasks.Run(WindowModel.Initialize);
        }
    }

    protected override async void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        await WindowModel.OnViewExited();
    }
}