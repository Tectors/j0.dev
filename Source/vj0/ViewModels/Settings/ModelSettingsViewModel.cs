using CommunityToolkit.Mvvm.ComponentModel;

using CUE4Parse_Conversion.Meshes;
using CUE4Parse_Conversion.Textures;
using CUE4Parse_Conversion.UEFormat.Enums;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.Nanite;

using vj0.Framework.Models;

namespace vj0.ViewModels.Settings;

public partial class ModelSettingsViewModel : ViewModelBase
{
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsUEFormat))]
    [NotifyPropertyChangedFor(nameof(IsActorXFormat))]
    private EMeshFormat _format;
    
    public bool IsUEFormat => Format == EMeshFormat.UEFormat;
    public bool IsActorXFormat => Format == EMeshFormat.ActorX;
    
    [ObservableProperty] private EFileCompressionFormat _compressionFormat;
    
    [ObservableProperty] private ESocketFormat _socketFormat = ESocketFormat.None;
    
    [ObservableProperty] private ENaniteMeshFormat _naniteFormat = ENaniteMeshFormat.OnlyNaniteLOD;
    
    [ObservableProperty] private EMaterialFormat _materialFormat = EMaterialFormat.FirstLayer;
    
    [ObservableProperty] private ETextureFormat _textureFormat = ETextureFormat.Png;
    
    [ObservableProperty] private ELodFormat _lodFormat;
    
    [ObservableProperty] private bool _embedMaterials;
    
    [ObservableProperty] private bool _saveMorphTargets = true;
}