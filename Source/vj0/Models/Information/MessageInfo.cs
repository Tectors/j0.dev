using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;

namespace vj0.Models.Information;

public partial class MessageInfo : ObservableObject {
    [ObservableProperty] private string _title = null!;
    [ObservableProperty] private string _message = null!;
    
    [ObservableProperty] private InfoBarSeverity _severity;
    
    [ObservableProperty] private bool _autoClose;
    [ObservableProperty] private string _id = null!;
    [ObservableProperty] private float _closeTime;

    [ObservableProperty] private bool _useButton;
    [ObservableProperty] private string _buttonTitle = null!;
    [ObservableProperty] private IRelayCommand _buttonCommand = null!;
}