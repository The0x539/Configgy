using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace Configgy
{
    public static class ConfigurationManager
    {
        internal static void RegisterConfiguraitonMenu(ConfigBuilder menu)
        {
            if (menus.Select(x => x.GUID).Contains(menu.GUID))
                throw new DuplicateNameException($"{nameof(ConfigBuilder)} GUID ({menu.GUID}) already exists! Using two ConfiggableMenus with the same GUID is not allowed.");

            menus.Add(menu);
            OnMenusChanged?.Invoke(GetMenus());
        }

        private static List<ConfigBuilder> menus = new List<ConfigBuilder>();

        internal static Action<ConfigBuilder[]> OnMenusChanged;

        internal static ConfigBuilder[] GetMenus()
        {
            return menus.ToArray();
        }

        internal static object GetObjectAtAddress(string address)
        {
            if (!Data.Config.Data.Configgables.ContainsKey(address))
                return null;

            return Data.Config.Data.Configgables[address];
        }

        internal static void SetObjectAtAddress(string address, object value)
        {
            Data.Config.Data.Configgables[address] = value;
            Initialize();
        }

        internal static void Save()
        {
            saveNextFrame = true;
        }

        internal static void SubMenuElementsChanged() 
        {
            OnMenusChanged?.Invoke(GetMenus());
        }

        private static bool initialized;
        private static bool saveNextFrame;
        private static IEnumerator SaveChecker()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (saveNextFrame)
                {
                    SaveData();
                    saveNextFrame = false;
                }
            }
        }

        private static void SaveData()
        {
            Data.Config.Save();
        }

        private static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            GameObject saveChecker = new GameObject("Configgy_Saver");
            BehaviourRelay br = saveChecker.AddComponent<BehaviourRelay>();
            br.StartCoroutine(SaveChecker());
            GameObject.DontDestroyOnLoad(saveChecker);
        }
    }
}
