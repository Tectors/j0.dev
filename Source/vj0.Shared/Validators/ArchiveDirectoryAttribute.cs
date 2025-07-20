using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace vj0.Shared.Validators;

public class ArchiveDirectoryAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string directory || directory.Length == 0)
        {
            return new ValidationResult("Archive directory is required.");
        }
        
        if (!Directory.Exists(directory))
        {
            return new ValidationResult("Archive directory must exist.");
        }

        var hasGameFiles = Directory.EnumerateFiles(directory)
            .Any(file =>
                file.EndsWith(".pak", System.StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".sig", System.StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".ucas", System.StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".utoc", System.StringComparison.OrdinalIgnoreCase));

        return hasGameFiles
            ? ValidationResult.Success
            : new ValidationResult("Archive directory is missing required game files (*.pak, *.sig, .ucas, .utoc).");
    }
}