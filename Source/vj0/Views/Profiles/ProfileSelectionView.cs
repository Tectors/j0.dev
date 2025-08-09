using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using vj0.Controls.Profiles;
using vj0.Framework.Models;
using vj0.Models.Enums;
using vj0.Services.Framework;
using vj0.ViewModels.Profiles;
using vj0.WindowModels;
using vj0.Windows;

namespace vj0.Views.Profiles;

public partial class ProfileSelectionView : ViewBase<ProfileSelectionViewModel>
{
    private bool UIThreadCompleted;
    
    public ProfileSelectionView() : base(ProfileSelectionVM)
    {
        InitializeComponent();
    
        ViewModel.ProfileListPanel = ProfileListPanel;
        ViewModel.WrapCard = WrapCard;
        ViewModel.HookEvents = HookEvents;
    
        ProfileListPanel.SizeChanged += (_, _) => UpdateCardWidths();

        Avalonia.Threading.Dispatcher.UIThread.Post(async void () =>
        {
            if (UIThreadCompleted) return;
            
            UIThreadCompleted = true;
            
            await ViewModel.RefreshAllAsync();
            UpdateCardWidths();
        });
        
        ScrollTopShadowRef.ScrollViewer = ProfileScrollViewer;
    }
    
    private void UpdateCardWidths()
    {
        if (ProfileListPanel?.Bounds.Width <= 0) return;

        var availableWidth = ProfileListPanel!.Bounds.Width;
        const double minCardWidth = 430;
        const double cardSpacing = 5;

        var cardsPerRow = Math.Max(1, (int)((availableWidth + cardSpacing) / (minCardWidth + cardSpacing)));

        var totalSpacing = cardSpacing * (cardsPerRow - 1);
        var totalCardWidth = availableWidth - totalSpacing;
        var cardWidth = totalCardWidth / cardsPerRow;

        foreach (var control in ProfileListPanel.Children)
        {
            var border = (Border)control;
            if (border.Child is ProfileCard)
            {
                border.Width = cardWidth;
            }
        }
    }

    private Border WrapCard(ProfileCard card) => new()
    {
        Child = card,
        Margin = new Thickness(0, 10, 0, 0),
        ClipToBounds = false,
        MinWidth = 400,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    private void HookEvents(ProfileCard card)
    {
        var window = this.GetVisualRoot() as MainWindow;
        if (window is null)
        {
            return;
        }
        
        var viewModel = (MainWindowModel)window.DataContext!;

        card.OnStart += async (_, _) =>
        {
            var profile = card.ViewModel.Profile;
            if (profile is not null)
            {
                await viewModel.StartProfileAsync(profile);
            }
        };

        card.OnEdit += (_, _) =>
        {
            var profile = card.ViewModel.Profile;
            if (profile is null) return;
            
            profile.OpenEditor(MainWM.Window);
        };

        card.OnDelete += async (_, _) =>
        {
            var profile = card.ViewModel.Profile;
            if (profile is null) return;
            
            var dialog = new ContentDialog
            {
                Title = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 12,
                    Children =
                    {
                        new ProfileSplashControl(1.5f)
                        {
                            DataContext = profile
                        },
                        new TextBlock
                        {
                            Text = $"Delete {profile.Name}?",
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 5)
                        }
                    }
                },
                Content = $"'{profile.Name}' will be permanently removed and cannot be restored.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Delete",
                PrimaryButtonCommand = new RelayCommand(() =>
                {
                    if (profile.FileName is not null && ViewModel.CardMap.TryGetValue(profile.FileName, out var cardToRemove))
                    {
                        var border = Enumerable.OfType<Border>(ProfileListPanel!.Children).FirstOrDefault(c => c.Child == cardToRemove);
                        if (border is not null)
                        {
                            ProfileListPanel.Children.Remove(border);
                        }
                
                        ViewModel.CardMap.Remove(profile.FileName);
                    }
            
                    ViewModel.IsEmpty = ViewModel.CardMap.Count == 0;

                    if (viewModel.CurrentProfile is not null && viewModel.CurrentProfile == profile)
                    {
                        viewModel.NavigateToStatus(AppStatus.Idle);
                    }

                    profile.Delete();
                })
            };
            
            await dialog.ShowAsync();
        };
    }
}
