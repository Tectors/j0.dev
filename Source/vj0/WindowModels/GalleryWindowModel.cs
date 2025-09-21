using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;

namespace vj0.WindowModels;

public partial class GalleryWindowModel : WindowModelBase
{
    [ObservableProperty] public string _title = "";
}
