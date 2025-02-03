using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Services
{
    public class OrderNumberGenerator : IOrderNumberGenerator
    {
        public string GenerateOrderNumber()
        {
            return $"ZAM_{DateTime.Now:yyyyMMdd}_{DateTime.Now.Ticks % 10000:0000}";
        }
    }
}
