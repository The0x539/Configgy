using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configgy.Patches
{
    [HarmonyPatch]
    public static class PreventCheatBind
    {
        //Block the cheat bind from being handled if we're pausing the game.
        [HarmonyPatch(typeof(CheatsManager), nameof(CheatsManager.HandleCheatBind)), HarmonyPrefix]
        public static bool OnHandleCheatBind()
        {
            return !Pauser.Paused;
        }
    }
}
