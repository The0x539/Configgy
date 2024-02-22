using System.IO;
using System.Reflection;

namespace Configgy
{
    internal static class Paths
    {
        public static string ExecutionPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);    
        public static string DataFolder => Path.Combine(ExecutionPath, "Configs");

    }
}
