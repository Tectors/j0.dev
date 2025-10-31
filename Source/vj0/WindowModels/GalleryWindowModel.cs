using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;

namespace vj0.WindowModels;

public partial class GalleryWindowModel : WindowModelBase
{
    [ObservableProperty] private string _title = "Title";
    [ObservableProperty] private string _description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut";
    
    [ObservableProperty] private bool _tag = true;
    [ObservableProperty] private string _tagText = "Tag";
}
