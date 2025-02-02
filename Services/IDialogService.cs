using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WoodworkManagementApp.Services
{
    public interface IDialogService
    {
        string OpenFileDialog(string title, string filter);
        string SaveFileDialog(string title, string filter, string defaultExtension);
        void ShowError(string message, string title = "Error");
        void ShowWarning(string message, string title = "Warning");
        void ShowInfo(string message, string title = "Information");
        bool ShowConfirmation(string message, string title = "Confirm");
    }
}
