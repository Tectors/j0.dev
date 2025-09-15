using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CUE4Parse.FileProvider.Objects;

namespace vj0.Models.Files;

public partial class FileTile : ObservableObject
{
    [ObservableProperty] private string _path;
    
    public string NameWithoutExtension => Path[(Path.LastIndexOf('/') + 1)..];
    public GameFile? GameFile { get; set; } 

    public FileTile(string path, GameFile? gameFile = null)
    {
        Path = path;
        GameFile = gameFile;
    }
    
    [RelayCommand]
    private void CopyPath()
    {
        App.CopyText(Path);
    }
}
