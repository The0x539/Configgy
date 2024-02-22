using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
