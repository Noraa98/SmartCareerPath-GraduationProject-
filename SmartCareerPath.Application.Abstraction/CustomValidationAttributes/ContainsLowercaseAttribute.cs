using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.CustomValidationAttributes
{
    public class ContainsLowercaseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return ValidationResult.Success;

            if (!str.Any(char.IsLower))
                return new ValidationResult("Must contain at least one lowercase letter");

            return ValidationResult.Success;
        }
    }
}
