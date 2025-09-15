using System;

using CommunityToolkit.Mvvm.ComponentModel;

using vj0.Core.Validators;

namespace vj0.Core.Framework.Base;

public partial class BaseMappingsContainer : ObservableValidator
{
    [NotifyDataErrorInfo]
    [File]
    [ObservableProperty] private string _path = string.Empty;
    
    [ObservableProperty] private bool _override;
    
    public BaseMappingsContainer Clone()
    {
        return new BaseMappingsContainer()
        {
            Path = Path,
            Override = Override
        };
    }

    private bool Equals(BaseMappingsContainer? other)
    {
        if (other is null) return false;

        return string.Equals(Path, other.Path, StringComparison.Ordinal) && Override == other.Override;
    }

    public override bool Equals(object? obj) => Equals(obj as BaseMappingsContainer);
    public override int GetHashCode() => HashCode.Combine(Path, Override);
}
