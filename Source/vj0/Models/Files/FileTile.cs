using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace vj0.Models.Files;

public partial class FileTile : ObservableObject
{
    [ObservableProperty] private string _path;
    
    public string NameWithoutExtension => Path[(Path.LastIndexOf('/') + 1)..];

    public FileTile(string path)
    {
        Path = path;
    }
    
    [RelayCommand]
    private async Task CopyPath()
    {
        await App.Clipboard.SetTextAsync(Path);
    }
}