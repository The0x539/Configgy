using BepInEx;
using Configgy.Assets;
using HarmonyLib;

namespace Configgy
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;

        private ConfigBuilder configgyConfig;

        private void Awake()
        {
            PluginAssets.Initialize();
            harmony = new Harmony(ConstInfo.GUID+".harmony");
            harmony.PatchAll();

            configgyConfig = new ConfigBuilder(ConstInfo.GUID, "Configgy");
            configgyConfig.Build();

            Logger.LogInfo($"Plugin {ConstInfo.NAME} is loaded!");
        }
    }
}
