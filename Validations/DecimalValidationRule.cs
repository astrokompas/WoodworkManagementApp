using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WoodworkManagementApp.Validations
{
    public class DecimalValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return new ValidationResult(false, "Field cannot be empty");

            if (!decimal.TryParse(value.ToString(), out decimal result))
                return new ValidationResult(false, "Please enter a valid number");

            if (result < 0)
                return new ValidationResult(false, "Price cannot be negative");

            return ValidationResult.ValidResult;
        }
    }
}
