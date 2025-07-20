using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.VirtualFileSystem;
using vj0.Extensions;

namespace vj0.Models.Files;

public enum ENodeType
{
    Folder,
    File
}

public partial class TreeItem : ObservableObject
{
    public GameFile? GameFile { get; set; } 
    
    [ObservableProperty]
    private string _archive = null!;
    
    [ObservableProperty]
    private string _mountPoint = null!;
    
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _filePath;
    
    public IBrush ItemBrush => 
        Name == "Engine" ? new SolidColorBrush(Color.Parse("#e86c0e"))
        : Name == "Config" || Name == "Slate" ? new SolidColorBrush(Color.Parse("#6e6e6e"))
        : Name == "Plugins" ? new SolidColorBrush(Color.Parse("#ff5959"))
        : Name == "Environments" ? new SolidColorBrush(Color.Parse("#35fc46"))
        : Name == "Athena" ? new SolidColorBrush(Color.Parse("#6500ff"))
        : Name == "Blueprints" || Name == "Blueprint" ? new SolidColorBrush(Color.Parse("#0059ff"))
        : Name == "TimeOfDay" || Name == "TODM" ? new SolidColorBrush(Color.Parse("#006eff"))
        : Name == "UI" ? new SolidColorBrush(Color.Parse("#ffc400"))
        : Name == "Sounds" ? new SolidColorBrush(Color.Parse("#ff3300"))
        : Name == "Animation" ? new SolidColorBrush(Color.Parse("#91ff00"))

        : new SolidColorBrush(Color.Parse("#0e91e8"));

    private IReadOnlyList<TreeItem>? _cachedFolderChildren;
    private readonly object _childrenLock = new();

    [ObservableProperty]
    private ENodeType _type;

    [ObservableProperty]
    private bool _selected;
    
    partial void OnSelectedChanged(bool value)
    {
        if (value)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                ExplorerVM.SelectFolder(this);
            });
        }
    }
    
    public static void CollapseRecursive(TreeItem item)
    {
        item.Expanded = false;

        if (item.FolderChildren is not { Count: > 0 }) return;
        
        foreach (var child in item.FolderChildren)
        {
            CollapseRecursive(child);
        }
    }

    [ObservableProperty]
    private bool _expanded;

    [ObservableProperty]
    private Bitmap? _fileBitmap;

    [ObservableProperty]
    private TreeItem? _parent;

    private readonly ConcurrentDictionary<string, TreeItem> _childrenLookup = new();

    public IReadOnlyList<TreeItem> FolderChildren
    {
        get
        {
            var localCache = _cachedFolderChildren;
            if (localCache != null)
                return localCache;

            lock (_childrenLock)
            {
                if (_cachedFolderChildren != null)
                    return _cachedFolderChildren;

                _cachedFolderChildren = _childrenLookup.Values
                    .Where(x => x.Type == ENodeType.Folder)
                    .OrderBy(x => x.Name, new CustomComparer<string>(ComparisonExtensions.CompareNatural))
                    .ToArray();

                return _cachedFolderChildren;
            }
        }
    }

    private int? _folderCount;
    public int FolderCount
    {
        get
        {
            if (_folderCount.HasValue)
                return _folderCount.Value;

            _folderCount = _childrenLookup.Values.Count(x => x.Type == ENodeType.Folder);
            return _folderCount.Value;
        }
    }

    public bool HasFolders => FolderCount > 0;
    
    public int AssetCount => _childrenLookup.Values.Count(x => x.Type == ENodeType.File);
    
    public bool HasAssets => AssetCount > 0;

    [ObservableProperty]
    private bool _isSorted;

    [ObservableProperty]
    private bool _childrenLoaded;

    public TreeItem(string name, ENodeType type, string filePath = "", TreeItem? parent = null, GameFile gameFile = null!)
    {
        Name = name;
        Type = type;
        FilePath = filePath;
        Parent = parent;
        GameFile = gameFile;
        
        if (GameFile is VfsEntry vfsEntry)
        {
            Archive = vfsEntry.Vfs.Name;
            MountPoint = vfsEntry.Vfs.MountPoint;
        }
    }

    public bool TryGetChild(string name, out TreeItem child)
    {
        return _childrenLookup.TryGetValue(name, out child!);
    }

    public void AddChild(string name, TreeItem child)
    {
        if (_childrenLookup.TryAdd(name, child))
        {
            InvalidateCaches();
        }
    }

    private void InvalidateCaches()
    {
        lock (_childrenLock)
        {
            _cachedFolderChildren = null;
        }
    }
    
    public Task<IReadOnlyList<TreeItem>> GetAllChildrenAsync()
    {
        return Task.FromResult<IReadOnlyList<TreeItem>>(_childrenLookup.Values.ToArray());
    }

    [RelayCommand]
    private Task CopyPath()
    {
        return App.Clipboard.SetTextAsync(FilePath);
    }
}
