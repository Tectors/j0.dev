using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using vj0.Framework.Models;
using vj0.Core.Extensions;

namespace vj0.WindowModels;

public enum TagType
{
    New,
    Update,
    Developmental,
    
    Unknown
}

public partial class GalleryWindowModel : WindowModelBase
{
    [ObservableProperty] private string _title = "Title";
    [ObservableProperty] private string _description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut";
    
    [ObservableProperty] private bool _tag = true;

    [ObservableProperty] private TagType _tagType = TagType.Unknown;
    [ObservableProperty] private string _tagText = "Tag";
    
    [ObservableProperty] private IBrush _tagForeground = new SolidColorBrush(Color.Parse("#00d5ff"));
    [ObservableProperty] private IBrush _tagBackground = new SolidColorBrush(Color.Parse("#083a45"));

    [ObservableProperty] private string _secondaryButtonText = "Dismiss";
    [ObservableProperty] private string _primaryButtonText = "Got it";
    [ObservableProperty] private bool _primaryButtonEnabled = true;
    [ObservableProperty] private bool _secondaryButtonEnabled = true;
    
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private bool _isUpdate;
    [ObservableProperty] private bool _isDevelopmental;

    public event Action? OnPrimaryButtonClick;
    
    partial void OnTagTypeChanged(TagType value)
    {
        IsNew = value == TagType.New;
        IsUpdate = value == TagType.Update;
        IsDevelopmental = value == TagType.Developmental;
        
        TagText = value switch
        {
            TagType.New => "New",
            TagType.Update => "Update",
            TagType.Developmental => "Developmental",
            _ => "Tag"
        };
        
        var foregroundColor = value switch
        {
            TagType.New => Color.Parse("#00d5ff"),
            TagType.Update => Color.Parse("#ffa12e"),
            TagType.Developmental => Color.Parse("#0dff6a"),
            _ => Color.Parse("#aaaaaa")
        };

        TagForeground = new SolidColorBrush(foregroundColor);
        TagBackground = new SolidColorBrush(ColorExtensions.DarkenColor(foregroundColor, 0.4));
    }

    public virtual void InvokePrimaryButtonClick()
    {
        OnPrimaryButtonClick?.Invoke();
    }
}
