using System;
using System.Linq;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

using CUE4Parse.Encryption.Aes;

namespace vj0.Core.Framework.CUEParse;

public partial class EncryptionKey : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _guid = "";
    [ObservableProperty] private string _key = "";

    public static bool IsValidKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return true;
        }

        if (key.Contains(' '))
        {
            return false;
        }

        key = key.Trim();

        return (key.Length == 66 && key.StartsWith("0x") && key[2..].All(Uri.IsHexDigit)) || (key.Length == 64 && key.All(Uri.IsHexDigit));
    }

    [JsonIgnore] public bool IsValid => IsValidKey(Key);
    [JsonIgnore] public FAesKey AESKey => new(Key);

    private bool Equals(EncryptionKey? other)
    {
        if (other is null) return false;

        return string.Equals(Name, other.Name, StringComparison.Ordinal) && string.Equals(Guid, other.Guid, StringComparison.Ordinal) && string.Equals(Key, other.Key, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as EncryptionKey);
    public override int GetHashCode() => HashCode.Combine(Name, Guid, Key);
}
