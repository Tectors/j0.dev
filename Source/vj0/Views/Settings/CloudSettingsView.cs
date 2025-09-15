using vj0.Framework.Models;
using vj0.Services.Framework;
using vj0.ViewModels.Settings;

namespace vj0.Views.Settings;

public partial class CloudSettingsView : ViewBase<CloudSettingsViewModel>
{
    public CloudSettingsView() : base(AppServices.Settings.Cloud)
    {
        InitializeComponent();
    }
}
