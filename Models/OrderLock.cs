using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodworkManagementApp.Models
{
    public class OrderLock
    {
        public string OrderNumber { get; set; }
        public string UserName { get; set; }
        public DateTime LockTime { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
