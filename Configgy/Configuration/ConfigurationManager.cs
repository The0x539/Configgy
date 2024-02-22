using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        internal static event Action<ConfigBuilder[]> OnMenusChanged;

        internal static ConfigBuilder[] GetMenus()
        {
            return menus.ToArray();
        }

        internal static void SubMenuElementsChanged() 
        {
            OnMenusChanged?.Invoke(GetMenus());
        }
    }
}
