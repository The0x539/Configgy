using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Configgy
{
    internal static class Paths
    {
        public static string ExecutionPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);    
        public static string DataFolder => Path.Combine(ExecutionPath, "Configs");

    }
}
