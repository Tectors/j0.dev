using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

using CUE4Parse.UE4.VirtualFileSystem;

using DynamicData;

using FluentAvalonia.UI.Controls;

using vj0.Framework.Models;
using vj0.Models.Files;
using vj0.Core.Extensions;
using vj0.ViewModels;

namespace vj0.Views;

public class ExplorerPlaceholder : ViewBase<ExplorerViewModel>;

public partial class ExplorerView : ViewBase<ExplorerViewModel>
{
    public ExplorerView() : base(ExplorerVM)
    {
        InitializeComponent();
    }

    private bool IsTreeViewAttached;
    
    private void TreeView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (IsTreeViewAttached || sender is not TreeView treeView) return;
        IsTreeViewAttached = true;

        treeView.AddHandler(PointerPressedEvent, OnTreeViewPointerPressed, RoutingStrategies.Tunnel);
    }

    private DateTime _lastClickTime = DateTime.MinValue;
    private TreeItem? _lastClickedItem;

    private static readonly TimeSpan DoubleClickThreshold = TimeSpan.FromMilliseconds(300);

    private void OnTreeViewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Visual visual) return;

        var item = visual.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        if (item?.DataContext is not TreeItem treeItem) return;
        if (e.GetCurrentPoint(null).Properties.IsRightButtonPressed)
        {
            return;
        }
        
        var now = DateTime.UtcNow;
        if (_lastClickedItem == treeItem && now - _lastClickTime < DoubleClickThreshold)
        {
            _lastClickedItem = null;
            HandleTreeItemDoubleClick(treeItem);

            e.Handled = true;
            return;
        }

        _lastClickTime = now;
        _lastClickedItem = treeItem;
    }

    private static void HandleTreeItemDoubleClick(TreeItem item)
    {
        if (item is { Type: ENodeType.Folder, HasFolders: true })
        {
            item.Expanded = !item.Expanded;
        }
    }

    /* ReSharper disable once UnusedMember.Local */
    private void CollapseAll(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var root in ViewModel.TreeViewCollection)
            {
                TreeItem.CollapseRecursive(root);
            }

            var temp = ViewModel.TreeViewCollection.ToList();
            ViewModel.TreeViewCollection.Clear();
            ViewModel.TreeViewCollection.AddRange(temp);

            if (temp.Count <= 1) return;
            
            ViewModel.SelectFolder(temp[1]);
            temp[1].Selected = true;
        });
    }

    private void OnFileItemDoubleTapped(object? sender, TappedEventArgs e)
    {
    }

    private void OnBreadcrumbItemPressed(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is not TreeItem treeItem) return;
        
        ViewModel.LoadTreeItems(treeItem);
    }

    private void FileTree_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is TreeView { SelectedItem: TreeItem selectedItem })
        {
            ViewModel.FlatViewJumpTo(selectedItem.FilePath);
        }
    }
    
    private void OnFileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
        {
            return;
        }
        
        var selectedItems = listBox.SelectedItems;
    
        foreach (var item in selectedItems!)
        {
            if (item is FileTile treeView)
            {
                if (e.RemovedItems.Contains(item))
                {
                    continue;
                }
                
                if (treeView.GameFile is VfsEntry vfsEntry)
                {
                    ViewModel.SelectedItemArchive = vfsEntry.Vfs.Name;
                    ViewModel.SelectedItemArchiveVersion = vfsEntry.Vfs.Ver.ToString();
                    ViewModel.SelectedItemMountPoint = vfsEntry.Vfs.MountPoint;
                    
                    ViewModel.SelectedItemOffset = $"0x{vfsEntry.Offset:X}";
                    ViewModel.SelectedItemSize = StringExtensions.GetReadableSize(treeView.GameFile.Size);
                    ViewModel.SelectedItemCompressionMethod = treeView.GameFile.CompressionMethod.ToString();
                    ViewModel.SelectedItemIsEncrypted = treeView.GameFile.IsEncrypted.ToString();
                }
            }
        }
    }
}
