using Configgy.Assets;
using HarmonyLib;
using UnityEngine;

namespace Configgy.Patches
{
    [HarmonyPatch(typeof(CanvasController))]
    public static class InstanceUI
    {
        public static RectTransform CanvasRect { get; private set; }

        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void OnStart(CanvasController __instance)
        {
            CanvasRect = __instance.GetComponent<RectTransform>();
            GameObject.Instantiate(PluginAssets.ConfigurationMenu, CanvasRect);
        }
    }
}
