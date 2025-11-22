using CUE4Parse_Conversion;
using CUE4Parse_Conversion.Meshes;
using CUE4Parse_Conversion.Sounds;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Meshes;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Serilog;
using vj0.Core;
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
    private static bool IsBaseProfileReady => MainProfile != null && MainProfile!.Provider.Files.Count > 0;
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
    
    /* Request to retrieve all HLOD paths */
    [HttpGet("hlod/paths")]
    public ActionResult GetHLODPaths()
    {
        if (!IsBaseProfileReady) return new BadRequestObjectResult(JsonConvert.SerializeObject(new
        {
            reason = "Not initialized yet"
        }, Formatting.Indented));
        
        List<string> paths = [];

        paths.AddRange(
        MainProfile?.Provider!.Files.Values!
            .Select(a => a?.PathWithoutExtension)
            .Where(p =>
                p.Contains("/HLOD/") &&
                p.Contains("/Maps/") &&
                !p.EndsWith(".o"))
            .Distinct()
        );

        return new OkObjectResult(JsonConvert.SerializeObject(new
        {
            paths
        }, Formatting.Indented));
    }

    /* Normal Export */
    [HttpGet("export")]
    public ActionResult Get(bool raw, string? path, string? export_name, string? export_type)
    {
        if (!IsBaseProfileReady || path is null) return BadRequest();

        var contentType = Request.Headers.ContentType;
        path = path.SubstringBefore('.');
        
        /* Find the profile that'll have this asset */
        var profile = FindBaseProfileForPath(path, found: out var found);
        if (!found) return new NotFoundResult();
        
        var provider = profile.Provider;
        provider.TryLoadPackageObject(path, export: out var localObject);
        provider.TryLoadPackage(path, out var package);

        if (package is not null)
        {
            localObject ??= package.ExportsLazy[0].Value;

            if (!string.IsNullOrEmpty(export_name))
            {
                foreach (var export in package.ExportsLazy)
                {
                    var uObject = export.Value;
                    if (uObject is null || uObject.Name != export_name) continue;
                    
                    localObject = uObject;
                    
                    if (raw)
                    {
                        return new OkObjectResult(JsonConvert.SerializeObject(new
                        {
                            exports = (object[])[
                                localObject
                            ]
                        }, Formatting.Indented));
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(export_type))
            {
                var exports = new List<UObject>();
                
                foreach (var export in package.ExportsLazy)
                {
                    var uObject = export.Value;
                    if (uObject is null || uObject.ExportType != export_type) continue;
                    
                    localObject = uObject;
                    exports.Add(uObject);
                }

                if (raw)
                {
                    return new OkObjectResult(JsonConvert.SerializeObject(new
                    {
                        exports
                    }, Formatting.Indented));
                }
            }
        }

        /* Return a raw export */
        if (raw) return HandleRawExport(path, provider);

        /* Switch on Class Type */
        return localObject switch
        {
            UTexture texture => ProcessTexture(texture, contentType!),
            USoundWave wave => ProcessSoundWave(wave),
            UStaticMesh staticMesh => ProcessStaticMesh(staticMesh),
            _ => HandleRawExport(path, provider)
        };
    }
    
    private ObjectResult ProcessStaticMesh(UStaticMesh staticMesh)
    {
        var exporterOptions = new ExporterOptions
        {
            MeshFormat = EMeshFormat.ActorX,
            ExportMaterials = false,
            ExportMorphTargets = false
        };

        var exporter = new MeshExporter(staticMesh, exporterOptions);

        var input = staticMesh.GetPathName();
        input = input.Substring(0, input.LastIndexOf(".", StringComparison.Ordinal));
            
        var directory = Path.Combine(Globals.RuntimeFolder.FullName, input);
        var output = new FileInfo(directory + ".pskx");

        if (!output.Exists)
        {
            Directory.CreateDirectory(path: Path.GetDirectoryName(directory)!);
        }

        var newLod = exporter.MeshLods[0];
        var newFilePath = newLod.FileName[..newLod.FileName.LastIndexOf('/')];
        
        newFilePath = newFilePath[..newFilePath.LastIndexOf('/')] + "/" + staticMesh.Name + ".pskx";

        newLod = new Mesh(newFilePath, newLod.FileData, newLod.Materials);
        newLod.TryWriteToDir(new DirectoryInfo(Globals.RuntimeFolder.FullName), out var label, out var filePath);

        return new OkObjectResult(JsonConvert.SerializeObject(new
        {
            path = filePath
        }, Formatting.Indented));
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
                exports = new { texture }
            });
        }

        return File(textureData.Encode(ETextureFormat.Png, false, out _), "image/png");
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
                exports = new[] { wave }
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
                exports = finalExports
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
