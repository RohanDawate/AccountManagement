using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AccountManagement.Infra.Logging
{
    public static class LoggerFileNameResolver
    {
        public static string Resolve(string contextName = "")
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            var assemblyName = AppDomain.CurrentDomain.FriendlyName;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var baseName = string.IsNullOrWhiteSpace(contextName)
                ? $"{processName}_{assemblyName}_{timestamp}"
                : $"{contextName}_{timestamp}";

            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);

            return Path.Combine(logDir, $"{baseName}.log");
        }
    }
}
