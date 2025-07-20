using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace vj0.Controls;

public partial class KeybindDisplay : UserControl
{
    private static readonly StyledProperty<string> KeybindProperty = AvaloniaProperty.Register<KeybindDisplay, string>(nameof(Keybind));

    public string Keybind
    {
        get => GetValue(KeybindProperty);
        set => SetValue(KeybindProperty, value);
    }

    public ObservableCollection<string> Keys { get; } = [];

    public KeybindDisplay()
    {
        InitializeComponent();
        DataContext = this;

        this.GetObservable(KeybindProperty).Subscribe(UpdateKeys);
    }

    private void UpdateKeys(string? gesture)
    {
        Keys.Clear();

        if (string.IsNullOrWhiteSpace(gesture))
        {
            return;
        }

        var parts = gesture.Split('+');

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            Keys.Add(string.IsNullOrEmpty(trimmed) ? "+" : trimmed);
        }
    }
}