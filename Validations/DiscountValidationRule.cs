using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WoodworkManagementApp.Validations
{
    public class DiscountValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return new ValidationResult(false, "Field cannot be empty");

            if (!int.TryParse(value.ToString(), out int result))
                return new ValidationResult(false, "Please enter a valid number");

            if (result < 0 || result > 100)
                return new ValidationResult(false, "Discount must be between 0 and 100");

            return ValidationResult.ValidResult;
        }
    }
}
