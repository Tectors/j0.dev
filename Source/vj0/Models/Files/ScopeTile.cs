using CommunityToolkit.Mvvm.ComponentModel;
using CUE4Parse.UE4.Objects.Core.Misc;

namespace vj0.Models.Files;

public partial class ScopeTile : ObservableObject
{
    [ObservableProperty] private string _name = null!;
    
    [ObservableProperty] private bool _isEncrypted;
    [ObservableProperty] private bool _isDecrypted;
    
    /* Purely UI */
    [ObservableProperty] private bool _selected;
    
    [ObservableProperty] private long _length;
    [ObservableProperty] private int _fileCount;
    [ObservableProperty] private string _mountPoint = null!;
    [ObservableProperty] private FGuid _guid;
}