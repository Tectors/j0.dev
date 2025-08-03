using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
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
    private string _archiveVersion = null!;
    
    [ObservableProperty]
    private string _mountPoint = null!;
    
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _filePath;
    
    private IBrush? _itemBrush;
    public IBrush ItemBrush => _itemBrush ??= GetItemBrush();

    public IBrush GetItemBrush()
    {
        return Name switch
        {
            "Engine" => new SolidColorBrush(Color.Parse("#e86c0e")),
            "Localization" => new SolidColorBrush(Color.Parse("#575757")),
            "Movies" => new SolidColorBrush(Color.Parse("#edde11")),
            "Weapons" => new SolidColorBrush(Color.Parse("#e3496a")),
            "Config" or "Slate" => new SolidColorBrush(Color.Parse("#6e6e6e")),
            "Plugins" => new SolidColorBrush(Color.Parse("#ff5959")),
            "Environments" => new SolidColorBrush(Color.Parse("#35fc46")),
            "Athena" => new SolidColorBrush(Color.Parse("#6500ff")),
            "Blueprints" or "Blueprint" => new SolidColorBrush(Color.Parse("#0ee1e8")),
            "TimeOfDay" or "TODM" => new SolidColorBrush(Color.Parse("#006eff")),
            "UI" => new SolidColorBrush(Color.Parse("#ffc400")),
            "Sounds" => new SolidColorBrush(Color.Parse("#ff3300")),
            "BRCosmetics" => new SolidColorBrush(Color.Parse("#6e6e6e")),
            "GameFeatures" => new SolidColorBrush(Color.Parse("#6e6e6e")),
            "Animation" => new SolidColorBrush(Color.Parse("#91ff00")),
            _ => new SolidColorBrush(Color.Parse("#0e98e8"))
        };
    }

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
    private TreeItem? _parent;

    private readonly ConcurrentDictionary<string, TreeItem> _childrenLookup = new();

    public IReadOnlyList<TreeItem> FolderChildren
    {
        get
        {
            var localCache = _cachedFolderChildren;
            if (localCache is not null)
            {
                return localCache;
            }

            lock (_childrenLock)
            {
                if (_cachedFolderChildren is not null)
                {
                    return _cachedFolderChildren;
                }

                _cachedFolderChildren = _childrenLookup.Values
                    .Where(x => x.Type == ENodeType.Folder)
                    .OrderBy(x => x.Name, new CustomComparer<string>(ComparisonExtensions.CompareNatural))
                    .ToArray();

                return _cachedFolderChildren;
            }
        }
    }
    
    public IReadOnlyList<TreeItem> AllChildren
    {
        get
        {
            return _cachedAllChildren ??= _childrenLookup.Values.ToArray();
        }
    }

    private int? _folderCount;
    public int FolderCount
    {
        get
        {
            if (_folderCount.HasValue)
            {
                return _folderCount.Value;
            }

            _folderCount = _childrenLookup.Values.Count(x => x.Type == ENodeType.Folder);
            return _folderCount.Value;
        }
    }

    private bool? _hasFolders;
    public bool HasFolders => _hasFolders ??= FolderCount > 0;

    private bool? _hasAssets;
    public bool HasAssets => _hasAssets ??= AssetCount > 0;
    
    private int? _assetCount;

    public int AssetCount
    {
        get
        {
            if (_assetCount.HasValue)
            {
                return _assetCount.Value;
            }

            _assetCount = _childrenLookup.Values.Count(x => x.Type == ENodeType.File);
            return _assetCount.Value;
        }
    }
    
    public string AssetCountText => HasAssets ? $"• {AssetCount} files" : string.Empty;
    
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

        ReCache();
    }

    public void ReCache()
    {
        if (GameFile is VfsEntry vfsEntry)
        {
            Archive = vfsEntry.Vfs.Name;
            ArchiveVersion = vfsEntry.Vfs.Ver.ToString();
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
    
    private IReadOnlyList<TreeItem>? _cachedAllChildren;

    public Task<IReadOnlyList<TreeItem>> GetAllChildrenAsync()
    {
        return Task.FromResult(_cachedAllChildren ??= _childrenLookup.Values.ToArray());
    }

    [RelayCommand]
    private void CopyPath()
    {
        App.CopyText(FilePath);
    }
    
    public void Refresh()
    {
        lock (_childrenLock)
        {
            _cachedFolderChildren = null;
            _cachedAllChildren = null;
            _assetCount = null;
            _folderCount = null;
            _hasAssets = null;
            _hasFolders = null;
        }

        _ = FolderChildren;
        _ = FolderCount;
        _ = AssetCount;
        _ = HasAssets;
        _ = HasFolders;

        OnPropertyChanged(nameof(FolderChildren));
        OnPropertyChanged(nameof(FolderCount));
        OnPropertyChanged(nameof(AssetCount));
        OnPropertyChanged(nameof(HasAssets));
        OnPropertyChanged(nameof(HasFolders));
    }
    
    public string GetFullPath()
    {
        var segments = new List<string>();
        var current = this;

        while (current is not null)
        {
            segments.Add(current.Name);
            current = current.Parent;
        }

        segments.Reverse();
        return string.Join('/', segments) + '/';
    }
}
