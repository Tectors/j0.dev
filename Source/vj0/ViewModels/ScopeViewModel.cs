using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.VirtualFileSystem;
using vj0.Framework.Models;
using vj0.Models.Files;

namespace vj0.ViewModels;

public partial class ScopeViewModel : ViewModelBase
{
    public ObservableCollection<ScopeTile> Files { get; } = new();
    
    [ObservableProperty] private ObservableCollection<FileTile> _selectedItems = [];
    [ObservableProperty] private ObservableCollection<TreeItem> _selectedTreeItems = [];
    [ObservableProperty] private ReadOnlyObservableCollection<FileTile> _viewCollection = new([]);
    [ObservableProperty] private ObservableCollection<TreeItem> _treeViewCollection = [];
    [ObservableProperty] private ObservableCollection<TreeItem> _fileViewStack = [];
    [ObservableProperty] private ObservableCollection<TreeItem> _fileViewCollection = [];
    
    private static readonly Regex FileRegex = new(@"^(?!global|pakchunk.+optional\-).+(pak|utoc)$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public void Reset()
    {
        Files.Clear();
    }
    
    public void Add(IAesVfsReader reader)
    {
        if (!FileRegex.IsMatch(reader.Name)) return;

        Files.Add(new ScopeTile
        {
            Name = reader.Name,
            Length = reader.Length,
            
            Guid = reader.EncryptionKeyGuid,
            
            IsEncrypted = reader.IsEncrypted,
            IsDecrypted = false,
        });

        Sort();
    }

    private void Sort()
    {
        var sorted = Files.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();

        for (var i = 0; i < sorted.Count; i++)
        {
            var currentIndex = Files.IndexOf(sorted[i]);
            if (currentIndex != i)
            {
                Files.Move(currentIndex, i);
            }
        }
    }
    
    public void Verify(IAesVfsReader reader)
    {
        if (Files.FirstOrDefault(x => x.Name == reader.Name) is not { } file) return;

        file.IsDecrypted = true;
        file.MountPoint = reader.MountPoint;
        file.FileCount = reader.FileCount;
    }

    public void Disable(IAesVfsReader reader)
    {
        if (Files.FirstOrDefault(x => x.Name == reader.Name) is not { } file) return;
        
        file.IsDecrypted = false;
    }

    [RelayCommand]
    private void Load()
    {
        Scope(Files);
    }

    private static void Scope(IEnumerable<ScopeTile> directoryFiles)
    {
        var directoryFilter = directoryFiles.Where(f => f.Selected)
            .Select(f => f.Name)
            .ToHashSet();
        var hasFilter = directoryFilter.Any();

        var gameFileEntries = new Dictionary<string, GameFile>();

        foreach (var file in MainWM.CurrentProfile!.Provider.Files.Values)
        {
            if (file.IsUePackagePayload) continue;

            if (hasFilter)
            {
                if (file is VfsEntry entry && directoryFilter.Contains(entry.Vfs.Name))
                {
                    gameFileEntries[file.Path] = file;
                }
            }
            else
            {
                gameFileEntries[file.Path] = file;
            }
        }

        _ = ExplorerVM.BuildTreeAsync(gameFileEntries);
    }
}