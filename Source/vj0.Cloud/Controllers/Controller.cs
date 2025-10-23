using CUE4Parse_Conversion.Sounds;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Meshes;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Serilog;

using vj0.Core.Convertors;
using vj0.Core.Framework.Base;

/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
/* vj0 Cloud Controller */
/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

namespace vj0.Cloud.Controllers;

[Route("api")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class CloudApiController : ControllerBase
{
    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    private static IEnumerable<BaseProfile> SecondaryBaseProfiles = [];
    private static BaseProfile? MainProfile;
    private static bool IsBaseProfileReady => MainProfile!.Provider.Files.Count > 0;
    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

    public static void SetProfile(BaseProfile profile)
    {
        if (profile is not null)
        {
            Log.Information($"[vj0.Cloud]: MainProfile got updated to {profile.Name}");
        }

        MainProfile = profile;
    }
    
    public static void SetSecondaryProfile(BaseProfile profile)
    {
        var list = SecondaryBaseProfiles.ToList();

        if (list.Count > 0)
        {
            list[0] = profile;
        }
        else
        {
            list.Add(profile);
        }

        SecondaryBaseProfiles = list;
    }
    
    /* TODO: ................... ENDPOINTS START HERE ......................................................................... */
    
    /* Metadata request to retrieve information about this process */
    [HttpGet("metadata")]
    public ActionResult Get()
    {
        if (!IsBaseProfileReady) return new BadRequestObjectResult(JsonConvert.SerializeObject(new
        {
            reason = "Not initialized yet"
        }, Formatting.Indented));

        return new OkObjectResult(JsonConvert.SerializeObject(new
        {
            name = MainProfile?.Provider.ProjectName,
            major_version = MainProfile?.Version >= EGame.GAME_UE5_0 ? 5 : 4
        }, Formatting.Indented));
    }
    
    /* Normal Export */
    [HttpGet("export")]
    public ActionResult Get(bool raw, string? path)
    {
        if (!IsBaseProfileReady) return BadRequest();
        if (path is null) return BadRequest();
        
        var contentType = Request.Headers.ContentType;
        path = path.SubstringBefore('.');
        
        /* Find the profile that'll have this asset */
        var profile = FindBaseProfileForPath(path, found: out var found);
        if (!found) return new NotFoundResult();
        
        var provider = profile.Provider;
        provider.TryLoadPackageObject(path, export: out var localObject);

        /* Return a raw export */
        if (raw) return HandleRawExport(path, provider);

        /* Switch on Class Type */
        return localObject switch
        {
            UTexture texture => ProcessTexture(texture, contentType!),
            USoundWave wave => ProcessSoundWave(wave),
            _ => HandleRawExport(path, provider)
        };
    }

    /* Return a texture as a file / encoding */
    private ActionResult ProcessTexture(UTexture texture, string contentType)
    {
        if (texture.GetFirstMip()?.BulkData!.Data is { } mipData && contentType == "application/octet-stream")
        {
            return File(mipData, contentType);
        }

        var textureData = texture.Decode();
        if (textureData is null)
        {
            return StatusCode(500, new
            {
                errored = true,
                exceptionstring = "Invalid texture data, exported as json",
                jsonOutput = new { texture }
            });
        }

        return StatusCode(500);
    }

    /* Return a sound wave file format */
    private ActionResult ProcessSoundWave(USoundWave wave)
    {
        wave.Decode(true, out var audioFormat, out var data);
        
        if (data is null || string.IsNullOrEmpty(audioFormat))
        {
            return Conflict(new
            {
                errored = true,
                exceptionstring = "Invalid audio data, returned raw export as json",
                jsonOutput = new[] { wave }
            });
        }

        var mimeType = audioFormat.ToLower() switch
        {
            "wem" => "application/vnd.wwise.wem",
            "wav" => "audio/wav",
            "adpcm" => "audio/adpcm",
            "opus" => "audio/opus",
            _ => "audio/ogg"
        };

        return File(data, mimeType);
    }

    /* Handle raw exports */
    public ActionResult HandleRawExport(string path, BaseProvider provider)
    {
        try
        {
            var objectPath = $"{path.SubstringBefore('.')}.o.uasset";

            var pkg = provider.LoadPackage(path);
            var exports = pkg.GetExports().ToArray();
            var finalExports = new List<UObject>(exports);

            var mergedExports = new List<UObject>();
            if (provider.TryLoadPackage(objectPath, out var editorAsset))
            {
                foreach (var export in exports)
                {
                    var editorData = editorAsset.GetExportOrNull($"{export.Name}EditorOnlyData");
                    if (editorData is null)
                    {
                        continue;
                    }

                    export.Properties.AddRange(editorData.Properties);
                    mergedExports.Add(export);
                }

                finalExports.AddRange(editorAsset.GetExports()
                    .Where(editorExport => !mergedExports.Contains(editorExport)));
            }

            mergedExports.Clear();

            var converters = new Dictionary<Type, JsonConverter>
                { { typeof(FColorVertexBuffer), new FColorVertexBufferCustomConverter() } };
            var settings = new JsonSerializerSettings
                { ContractResolver = new FColorVertexBufferCustomResolver(converters!) };

            /* Serialize object, and return it indented */
            return new OkObjectResult(JsonConvert.SerializeObject(new
            {
                jsonOutput = finalExports
            }, Formatting.Indented, settings));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }
    
    /*
     * If the path exists on the main profile, it'll check if other profiles specifically override the main profile, if so it'll pick that, else it'll give the one found initially
     * If the path doesn't exist on a main profile, it'll cycle through each profile to find one that has the asset existing
     */
    private static BaseProfile FindBaseProfileForPath(string rawPath, out bool found)
    {
        var path = rawPath.SubstringBefore('.');
        found = false;
        
        if (MainProfile!.Provider.TryLoadPackage(path, package: out _))
        {
            found = true;

            foreach (var profile in SecondaryBaseProfiles)
            {
                if (!profile.IsInitialized) continue;
                if (!profile.Provider.TryLoadPackage(path, package: out var package)) continue;
                var assetType = package.GetExports().FirstOrDefault()?.ExportType;

                if (assetType is not null && profile.SecondaryAssetTypes.Contains(assetType, StringComparer.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }
        }
        else
        {
            foreach (var profile in SecondaryBaseProfiles)
            {
                if (!profile.IsInitialized) continue;
                if (!profile.Provider.TryLoadPackage(path, package: out _)) continue;
            
                found = true;
                
                return profile;
            }
        }
        
        found = true;

        return MainProfile;
    }
}
