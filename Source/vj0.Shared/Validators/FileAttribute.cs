using System.ComponentModel.DataAnnotations;
using System.IO;

namespace vj0.Shared.Validators;

public class FileAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string file || file.Length == 0)
        {
            return new ValidationResult("File is required.");
        }
        
        if (!File.Exists(file))
        {
            return new ValidationResult("File must exist.");
        }

        return ValidationResult.Success;
    }
}