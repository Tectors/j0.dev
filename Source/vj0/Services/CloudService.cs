using System.Threading.Tasks;

using Avalonia.Threading;

using FluentAvalonia.UI.Controls;

using vj0.Cloud;
using vj0.Framework;

namespace vj0.Services;

public class CloudService : IService
{
    public CloudWebAPI? API;
    
    public void Initialize()
    {
        if (API is null)
        {
            API = new CloudWebAPI();
            
            API.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(API.IsRunning))
                {
                    MainWM.UpdateAPIServiceStatusColor();
                }
            };
        
            API.OnError += OnError;
            API.OnInitialized += OnInitialized;
        }

        if (Settings.Cloud.RunHostedAPI)
        {
            Task.Run(() => API.Run());
        }
    }

    private void OnError(string Title, string Header, string Content)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Info.Message(Title, Content, InfoBarSeverity.Error, buttonCommand: Restart, buttonTitle: "Restart API", useButton: true);
        });
    }

    private void OnInitialized(string URL)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Info.Message("Started Cloud", URL, InfoBarSeverity.Success, closeTime: 0.5f);
        });
    }

    public void Restart()
    {
        Stop();
        Initialize();
    }

    public void Stop()
    {
        if (API is null)
        {
            return;
        }

        _ = API.StopAsync();
    }
}