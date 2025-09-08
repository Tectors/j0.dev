using System;
using System.IO;

using CUE4Parse.FileProvider.Vfs;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;

namespace vj0.Shared.Framework.Base;

public class BaseProvider : AbstractVfsFileProvider
{
    private readonly DirectoryInfo WorkingDirectory;
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true
    };

    public BaseProvider(string directory, VersionContainer? version = null) : base(version, StringComparer.OrdinalIgnoreCase)
    {
        WorkingDirectory = new DirectoryInfo(directory);
        SkipReferencedTextures = true;
    }

    public override void Initialize()
    {
        if (!WorkingDirectory.Exists) throw new DirectoryNotFoundException($"Profile installation folder doesn't exist: {WorkingDirectory.FullName}");
        
        RegisterFiles(WorkingDirectory);
    }

    private void RegisterFiles(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles("*.*", EnumerationOptions))
        {
            RegisterVfs(file.FullName, [ file.OpenRead() ], it => new FStreamArchive(it, File.OpenRead(it), Versions));
        }
    }
}