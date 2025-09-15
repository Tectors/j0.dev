using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using CommunityToolkit.Mvvm.ComponentModel;

using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Versions;

using vj0.Core.Framework.CUEParse;
using vj0.Core.Validators;

namespace vj0.Core.Framework.Base;

public enum EDetectedGameId
{
    None,
    Fortnite,
    Valorant
}

public enum EAudioFormatType
{
    [Description("Decompressed Data")]
    Decompressed,
    
    [Description("Raw Data")]
    Raw
}

/* This base profile class is used to separate what's needed to shared, and what's visual (located in vj0) */
/* This class is used by both vj0.Cloud and vj0 */
public partial class BaseProfile : ObservableValidator
{
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Profile Name is required.")]
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsNameEmpty))]
    private string _name = "";

    [NotifyDataErrorInfo]
    [ArchiveDirectory] 
    [ObservableProperty] private string _archiveDirectory = "";

    partial void OnArchiveDirectoryChanged(string value)
    {
        ArchiveDirectoryChanged();
    }

    protected virtual void ArchiveDirectoryChanged() { }

    partial void OnNameChanged(string value)
    {
        NameChanged();
    }
    
    protected virtual void NameChanged() { }

    [ObservableProperty] private BaseMappingsContainer _mappingsContainer = new();
    
    [ObservableProperty] private EncryptionContainer _encryption = new();
    
    [ObservableProperty] private EGame _version = EGame.GAME_UE5_LATEST;
    
    [ObservableProperty] private ETexturePlatform _texturePlatform = ETexturePlatform.DesktopMobile;
    
    [ObservableProperty] private EAudioFormatType _audioFormat = EAudioFormatType.Decompressed;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(FileID))]
    private string _fileName = "";

    [JsonIgnore] public string FileID => FileName.Split(".json")[0];
    
    /* Detection ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAutoDetected))]
    [NotifyPropertyChangedFor(nameof(IsArchivedGame))]
    private EDetectedGameId _autoDetectedGameId = EDetectedGameId.None;

    [JsonIgnore]
    public bool IsAutoDetected => AutoDetectedGameId != EDetectedGameId.None;
    
    [JsonIgnore]
    public bool IsArchivedGame =>
        !IsAutoDetected
        && ArchiveDirectory.Contains("Fortnite")
        && Regex.IsMatch(Name, @"^\d+\.\d+(\.\d+)?$");
    
    [JsonIgnore]
    public bool IsNameEmpty => string.IsNullOrEmpty(Name);
    
    [ObservableProperty] private bool _TexturesOnDemand;
    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    
    [JsonIgnore] public BaseProvider Provider = null!;
    
    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInitializedAndIsActive))] 
    private BaseProfileStatus _status = new();
    
    [JsonIgnore]
    private bool _isInitialized;

    [JsonIgnore]
    public bool IsInitialized
    {
        get => _isInitialized;
        set
        {
            if (_isInitialized == value) return;
            _isInitialized = value;
                
            OnPropertyChanged(nameof(IsNotInitializedAndIsActive));
        }
    }

    [JsonIgnore]
    public bool IsNotInitializedAndIsActive => !IsInitialized && Status.State == EProfileStatus.Active;
    
    /* For using more than one profile */
    [JsonIgnore] public List<string> SecondaryAssetTypes { get; init; } = [];

    public void CheckStatusNotifies()
    {
        Status.OnStateChange = () =>
        {
            OnPropertyChanged(nameof(IsNotInitializedAndIsActive));
        };
    }
}
