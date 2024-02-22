using BepInEx.Bootstrap;
using System.Linq;
using System.Reflection;

namespace Configgy.Configuration.AutoGeneration
{
    public static class BepinAutoGenerator
    {
        private static ConfigBuilder autoGenConfig;

        public static void Generate()
        {
            if (autoGenConfig != null)
                return;

            foreach (var plugin in Chainloader.PluginInfos.Values)
            {
                var info = plugin.Metadata;
                var configs = plugin.Instance.Config.Select(c => c.Value);

                var assembly = Assembly.GetAssembly(plugin.Instance.GetType());

            }

        }


    }
}
