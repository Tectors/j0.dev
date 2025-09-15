using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

using CUE4Parse.Encryption.Aes;

namespace vj0.Core.Framework.CUEParse;

public partial class EncryptionContainer : ObservableValidator
{
    [ObservableProperty] private string _mainKey = "0x0000000000000000000000000000000000000000000000000000000000000000";
    [ObservableProperty] private List<EncryptionKey> _keys = [];
    [ObservableProperty] private List<EncryptionKey> _unknownKeys = [];

    [JsonIgnore] public bool HasKeys => Keys.Count > 0;
    [JsonIgnore] public bool IsValid => MainKey.Length == 66 || HasKeys;
    
    [JsonIgnore] public FAesKey MainAESKey => new(MainKey);

    partial void OnMainKeyChanged(string value)
    {
        if (value == "")
        {
            MainKey = "0x0000000000000000000000000000000000000000000000000000000000000000";
        }
    }

    public EncryptionContainer Clone()
    {
        return new EncryptionContainer
        {
            MainKey = MainKey,
            Keys =
            [
                ..Keys.Select(k => new EncryptionKey
                {
                    Name = k.Name,
                    Key = k.Key,
                    Guid = k.Guid
                })
            ]
        };
    }

    private bool Equals(EncryptionContainer? other)
    {
        if (other is null) return false;

        return string.Equals(MainKey, other.MainKey, StringComparison.Ordinal) && Keys.SequenceEqual(other.Keys);
    }
    
    public override bool Equals(object? obj) => Equals(obj as EncryptionContainer);
    public override int GetHashCode() => HashCode.Combine(MainKey, Keys.Count);
}
