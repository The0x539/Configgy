using BepInEx;
using Configgy.Assets;
using HarmonyLib;
using HydraDynamics;

namespace Configgy
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    [BepInDependency("Hydraxous.HydraDynamics", BepInDependency.DependencyFlags.HardDependency)]
    [HydynamicsInfo(ConstInfo.NAME, ConstInfo.GUID, ConstInfo.VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;

        private void Awake()
        {
            PluginAssets.Initialize();
            harmony = new Harmony(ConstInfo.GUID+".harmony");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {ConstInfo.NAME} is loaded!");
        }
    }
}
