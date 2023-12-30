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

        public static bool UsingLatest = true;
        public static string LatestVersion { get; private set; } = ConstInfo.VERSION;

    

        private void Awake()
        {
            PluginAssets.Initialize();
            harmony = new Harmony(ConstInfo.GUID+".harmony");
            harmony.PatchAll();

            configgyConfig = new ConfigBuilder(ConstInfo.GUID, "Configgy");
            configgyConfig.Build();

            VersionCheck.CheckVersion(ConstInfo.GITHUB_VERSION_URL, ConstInfo.VERSION, (r, latest) =>
            {
                UsingLatest = r;
                if (!UsingLatest)
                    LatestVersion = latest;
            });

            Logger.LogInfo($"Plugin {ConstInfo.NAME} is loaded!");
        }
    }
}
