using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Models
{
    public class AppSettings
    {
        public string OrdersPath { get; set; }
        public string TemplatesPath { get; set; }

        public AppSettings()
        {
            OrdersPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WoodworkManagementApp",
                "Orders"
            );
            TemplatesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WoodworkManagementApp",
                "Templates"
            );
        }
    }
}
