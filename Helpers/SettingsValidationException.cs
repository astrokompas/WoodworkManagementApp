using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Helpers
{
    public class SettingsValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public SettingsValidationException(IEnumerable<string> errors)
            : base("Settings validation failed")
        {
            Errors = new List<string>(errors);
        }

        public SettingsValidationException(string message)
            : base(message)
        {
            Errors = new List<string> { message };
        }

        public SettingsValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = new List<string> { message };
        }
    }
}
