using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using vj0.ViewModels.Profiles;

namespace vj0.Views.Profiles;

public partial class ProfileCard : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    private double _scaledMoreUp = 1.0;
    public double ScaledMoreUp
    {
        get => _scaledMoreUp;
        private set
        {
            if (!(Math.Abs(_scaledMoreUp - value) > 0.0001)) return;
            
            _scaledMoreUp = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaledMoreUp)));
        }
    }
    
    public ProfileCardViewModel ViewModel { get; }

    public ProfileCard()
    {
        ViewModel = new ProfileCardViewModel();
        
        ViewModel.Profile!.Display.SetRandomGradient();
        ViewModel.Profile!.Name = "Game 1";
        ViewModel.Profile!.ArchiveDirectory = @"T:\Games\Game\Content\Paks";
        
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.Scale))
            {
                ScaledMoreUp = ViewModel.ScaledMoreUp;
            }
        };
        
        ViewModel.Initialize();
        DataContext = ViewModel;
        
        InitializeComponent();
    }

    public ProfileCard(ProfileCardViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        ViewModel.Initialize();
        
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.Scale))
            {
                ScaledMoreUp = ViewModel.ScaledMoreUp;
            }
        };

        ViewModel.OnStart += (_, _) => OnStart?.Invoke(this, EventArgs.Empty);
        ViewModel.OnEdit += (_, _) => OnEdit?.Invoke(this, EventArgs.Empty);
        ViewModel.OnDelete += (_, _) => OnDelete?.Invoke(this, EventArgs.Empty);

        DataContext = ViewModel;
    }

    public event EventHandler? OnStart;
    public event EventHandler? OnEdit;
    public event EventHandler? OnDelete;

    private void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        OnStart?.Invoke(this, EventArgs.Empty);
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        OnEdit?.Invoke(this, EventArgs.Empty);
    }

    private void DeleteProfile(object? sender, RoutedEventArgs e)
    {
        MoreOptionsButton.Flyout?.Hide();
        
        OnDelete?.Invoke(this, EventArgs.Empty);
    }

    private void OnCardPointerEntered(object? sender, PointerEventArgs e)
    {
        ViewModel.HoverEnter();
    }

    private void OnCardPointerExited(object? sender, PointerEventArgs e)
    {
        ViewModel.HoverExit();
    }
}