using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WoodworkManagementApp.Helpers
{
    public static class LoggerExtensions
    {
        public static ILogger<T> CreateLogger<T>(this ILogger logger)
        {
            var loggerFactory = new LoggerFactory();
            return loggerFactory.CreateLogger<T>();
        }
    }
}
