using Avalonia.Controls;

using vj0.Framework.Models;
using vj0.Services;
using vj0.WindowModels;

namespace vj0.Windows;

/* ~~~ GalleryWindow ~~~ */
public partial class GalleryWindow : WindowBase<GalleryWindowModel>
{
    private readonly NavigatorContext NavigatorContext = new();
    
    public GalleryWindow()
    {
        Setup();
    }
    
    /* ~~~ Setup Logic ~~~ */
    private void Setup()
    {
        InitializeComponent();
        captionButtons.Attach((VisualRoot as Window)!);

        NavigatorContext.Initialize(GalleryNavigationView);

        _ = WindowModel.Initialize();
        DataContext = WindowModel;
    }
}
