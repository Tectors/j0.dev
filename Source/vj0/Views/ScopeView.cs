using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using vj0.Framework.Models;
using vj0.Models.Files;
using vj0.ViewModels;

namespace vj0.Views;

public partial class ScopeView : ViewBase<ScopeViewModel>
{
    public ScopeView(): base(ScopeVM)
    {
        InitializeComponent();
    }
    
    private bool _isAttached;
    
    private void TreeView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_isAttached || sender is not TreeView treeView) return;
        _isAttached = true;

        treeView.AddHandler(PointerPressedEvent, OnTreeViewPointerPressed, RoutingStrategies.Tunnel);
        treeView.AddHandler(PointerReleasedEvent, OnTreeViewPointerReleased, RoutingStrategies.Tunnel);
    }
    
    private DateTime _lastClickTime = DateTime.MinValue;
    private TreeItem? _lastClickedItem;
    private TreeItem? _longPressItem;
    private DispatcherTimer? _longPressTimer;
    private bool _longPressHandled;

    private static readonly TimeSpan DoubleClickThreshold = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan LongPressThreshold = TimeSpan.FromSeconds(0.18);
    
    private void OnTreeViewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Visual visual) return;

        var item = visual.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        if (item?.DataContext is not TreeItem treeItem) return;

        var now = DateTime.UtcNow;
        if (_lastClickedItem == treeItem && (now - _lastClickTime) < DoubleClickThreshold)
        {
            _lastClickedItem = null;
            HandleTreeItemDoubleClick(treeItem);

            if (treeItem.HasAssets && !treeItem.HasFolders)
            {
                HandleTreeItemLongPress(treeItem);
            }

            e.Handled = true;
            return;
        }

        _lastClickTime = now;
        _lastClickedItem = treeItem;
        _longPressItem = treeItem;
        _longPressHandled = false;

        _longPressTimer?.Stop();
        _longPressTimer = new DispatcherTimer { Interval = LongPressThreshold };
        _longPressTimer.Tick += (_, _) =>
        {
            _longPressTimer?.Stop();
            if (_longPressItem != null)
            {
                _longPressHandled = true;
                /*if (_longPressItem.HasAssets)
                {
                    HandleTreeItemLongPress(_longPressItem);
                }*/
            }
        };
        _longPressTimer.Start();
    }
    
    
    private void OnTreeViewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _longPressTimer?.Stop();

        if (_longPressHandled)
        {
            e.Handled = true;
        }

        _longPressHandled = false;
        _longPressItem = null;
    }

    private void HandleTreeItemDoubleClick(TreeItem item)
    {
        if (item.Type == ENodeType.Folder && item.HasFolders)
        {
            item.Expanded = !item.Expanded;
        }
    }

    private void HandleTreeItemLongPress(TreeItem item)
    {
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

            if (temp.Count > 1)
            {
                // ViewModel.SelectFolder(temp[1]);
                temp[1].Selected = true;
            }
        });
    }

    private void OnFileItemDoubleTapped(object? sender, TappedEventArgs e)
    {
    }
}