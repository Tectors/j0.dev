using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CUE4Parse.FileProvider.Objects;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using vj0.Application;
using vj0.Framework.Models;
using vj0.Models.Files;
using vj0.Shared.Framework.Base;

namespace vj0.ViewModels;

public partial class ExplorerViewModel : ViewModelBase
{
    private readonly SourceCache<FileTile, string> _viewAssetCache = new(item => item.Path);
    private CancellationTokenSource? _buildTreeCancellation;
    private TaskCompletionSource? _initialFilterCompletion;

    private BaseProvider Provider => MainWM.CurrentProfile?.Provider!;

    public int AssetCount;
    
    [ObservableProperty] private bool _isFolderView = true;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _searchFilter = string.Empty;
    [ObservableProperty] private bool _useRegex;
    [ObservableProperty] private bool _isLoading = true;

    [ObservableProperty] private ObservableCollection<FileTile> _selectedItems = [];
    [ObservableProperty] private ObservableCollection<TreeItem> _selectedTreeItems = [];
    [ObservableProperty] private ReadOnlyObservableCollection<FileTile> _viewCollection = new([]);
    [ObservableProperty] private ObservableCollection<TreeItem> _treeViewCollection = [];
    [ObservableProperty] private ObservableCollection<TreeItem> _fileViewStack = [];
    [ObservableProperty] private ObservableCollection<TreeItem> _fileViewCollection = [];
    
    [ObservableProperty]
    private ObservableCollection<FileTile> _flatViewFiles = [];
    
    [ObservableProperty]
    private int _selectedItemPackageCount;

    [ObservableProperty]
    private int _selectedItemFolderCount;
    
    [ObservableProperty]
    private string _selectedItemArchive = "";
    
    [ObservableProperty]
    private string _selectedItemMountPoint = "";
    
    public override Task Initialize()
    {
        if (!Globals.IsReadyToExplore) return base.Initialize();

        IsLoading = true;

        var assetFilter = this
            .WhenAnyValue<ExplorerViewModel, (string filter, bool regex), string, bool>(x => x.SearchFilter, x => x.UseRegex, (filter, regex) => (filter, regex))
            .Throttle(TimeSpan.FromMilliseconds(150))
            .DistinctUntilChanged()
            .Select(CreateAssetFilter);

        _initialFilterCompletion = new TaskCompletionSource();

        _viewAssetCache.Connect()
            .Filter(assetFilter)
            .SortAndBind(out var flatCollection, SortExpressionComparer<FileTile>.Ascending(x => x.Path))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (!_initialFilterCompletion!.Task.IsCompleted)
                {
                    _initialFilterCompletion.SetResult();
                }
            });

        ViewCollection = flatCollection;

        return base.Initialize();
    }

    public async Task FinalizeWhenProviderExplorerReady(Func<bool>? cancellationCheck = null)
    {
        if (!Globals.IsReadyToExplore) return;

        IsLoading = true;

        var fileTiles = Provider.Files
            .AsParallel()
            .WithCancellation(CancellationToken.None)
            .Where(pair => IsValidAssetPath(pair.Key) && !pair.Value.IsUePackagePayload)
            .Select(pair => new FileTile(pair.Key))
            .ToList();
        
        _viewAssetCache.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(fileTiles);
        });
        
        await BuildTreeAsync();
        IsLoading = false;
    }

    public async Task BuildTreeAsync(IReadOnlyDictionary<string, GameFile> files = null!)
    {
        _buildTreeCancellation?.Cancel();
        _buildTreeCancellation = new CancellationTokenSource();
        var token = _buildTreeCancellation.Token;

        try
        {
            await Task.Run((Func<Task?>)(async () =>
            {
                var rootMap = new ConcurrentDictionary<string, TreeItem>();

                const int batchSize = 2000;
                
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
                };

                if (files == null)
                {
                    files = Provider.Files;
                }

                var allPairs = files
                    .Where(pair => IsValidAssetPath(pair.Key) && !pair.Value.IsUePackagePayload)
                    .ToArray();

                AssetCount = allPairs.Length;

                for (int i = 0; i < allPairs.Length; i += batchSize)
                {
                    token.ThrowIfCancellationRequested();
                    var batch = allPairs.AsSpan(i, Math.Min(batchSize, allPairs.Length - i));
    
                    Parallel.ForEach(batch.ToArray(), parallelOptions, pair =>
                    {
                        token.ThrowIfCancellationRequested();
                        ProcessPathOptimized(pair.Key, pair.Value, rootMap);
                    });

                    if (i % (batchSize * 10) == 0)
                    {
                        await Task.Delay(1, token);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var sortedRoots = rootMap
                        .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(x => x.Value)
                        .Where(x => x.HasAssets || x.HasFolders)
                        .ToArray();

                    TreeViewCollection = new ObservableCollection<TreeItem>(sortedRoots);

                    if (sortedRoots.Length > 0)
                    {
                        LoadTreeItems(sortedRoots[0]);

                        if (sortedRoots.Length > 1)
                        {
                            sortedRoots[1].Selected = true;
                        }
                    }
                }, DispatcherPriority.Background);

            }), token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building tree: {ex.Message}");
        }
    }

    private static void ProcessPathOptimized(string path, GameFile gameFile, ConcurrentDictionary<string, TreeItem> rootMap)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var rootName = parts[0];
        var rootNode = rootMap.GetOrAdd(rootName, static rn => new TreeItem(rn, ENodeType.Folder, rn));

        var parent = rootNode;

        for (int i = 1; i < parts.Length; i++)
        {
            var name = parts[i];
            var isFile = i == parts.Length - 1;
            var fullPath = string.Join("/", parts, 0, i + 1);

            if (!parent.TryGetChild(name, out var child))
            {
                child = new TreeItem(name, isFile ? ENodeType.File : ENodeType.Folder, fullPath, parent, gameFile);
                parent.AddChild(name, child);
            }

            if (isFile)
            {
                child.GameFile = gameFile;
            }

            parent = child;
        }
    }

    private bool IsValidAssetPath(string path)
    {
        return !string.IsNullOrEmpty(path) &&
               !path.Contains(".o.") &&
               !path.Contains("/_verse/");
    }

    private Func<FileTile, bool> CreateAssetFilter((string filter, bool useRegex) input)
    {
        var (filter, useRegex) = input;

        if (string.IsNullOrWhiteSpace(filter))
        {
            return _ => true;
        }

        if (useRegex)
        {
            try
            {
                var regex = new Regex(filter, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return asset => regex.IsMatch(asset.Path);
            }
            catch
            {
                return _ => false;
            }
        }

        var lowerFilter = filter.ToLowerInvariant();
        return asset => asset.Path.Contains(lowerFilter, StringComparison.OrdinalIgnoreCase);
    }

    public void Reset()
    {
        SearchText = string.Empty;
        SearchFilter = string.Empty;
        UseRegex = false;
    }

    public void FlatViewJumpTo(string directory)
    {
        IsFolderView = false;

        if (!directory.EndsWith("/"))
        {
            directory += "/";
        }

        var matchingFiles = _viewAssetCache.Items
            .Where(tile =>
                tile.Path.StartsWith(directory, StringComparison.OrdinalIgnoreCase) &&
                !tile.Path.Substring(directory.Length).Contains('/'))
            .OrderBy(tile =>
            {
                var lastSlash = tile.Path.LastIndexOf('/');
                return lastSlash >= 0 ? tile.Path[(lastSlash + 1)..] : tile.Path;
            }, StringComparer.OrdinalIgnoreCase)
            .ToList();

        FlatViewFiles = new ObservableCollection<FileTile>(matchingFiles);
        SelectedItems = new ObservableCollection<FileTile>(matchingFiles);
        
        DispatcherTimer.RunOnce(() => IsFolderView = true, TimeSpan.FromSeconds(5));
    }
    
    public void SelectFolder(TreeItem item)
    {
        SelectedTreeItems = [item];
    }
    
    partial void OnSelectedTreeItemsChanged(ObservableCollection<TreeItem> value)
    {
        if (value.Count > 0)
        {
            var selected = value[0];
            
            SelectedItemPackageCount = selected.AssetCount;
            SelectedItemFolderCount = selected.FolderCount;
            SelectedItemArchive = selected.Archive;
            SelectedItemMountPoint = selected.MountPoint;
        }
        else
        {
            SelectedItemPackageCount = 0;
            SelectedItemFolderCount = 0;
            SelectedItemArchive = "";
            SelectedItemMountPoint = "";
        }
    }

    private void LoadTreeItems(TreeItem item)
    {
        Task.Run((Func<Task?>)(async () =>
        {
            var children = await item.GetAllChildrenAsync();
            var sortedChildren = children
                .OrderBy(x => x.Type == ENodeType.File)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                FileViewCollection = new ObservableCollection<TreeItem>(sortedChildren);
            }, DispatcherPriority.Background);
        }));

        var newStack = new List<TreeItem>();
        var parent = item;

        while (parent != null)
        {
            newStack.Insert(0, parent);
            parent = parent.Parent;
        }

        FileViewStack = new ObservableCollection<TreeItem>(newStack);
    }
}