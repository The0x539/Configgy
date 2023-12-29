using Configgy.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Configgy.UI
{
    public class ConfigurationMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform contentbody;

        private ConfigurationPage rootPage;
        private Dictionary<string,ConfigurationPage> pageManifest = new Dictionary<string,ConfigurationPage>();

        private bool menuBuilt = false;

        [Configgable(displayName:"Open Config Menu")]
        private static ConfigKeybind cfgKey = new ConfigKeybind(KeyCode.Backslash);

        private GameObject[] menus;

        ConfigurationPage lastOpenPage;

        private static event Action onGlobalOpen;
        private static event Action onGlobalClose;
        private bool menuOpen = false;


        private void Awake()
        {
            if (!menuBuilt)
                BuildMenus(ConfigurationManager.GetMenus());

            onGlobalOpen += OpenMenu;
            onGlobalClose += CloseMenu;
            ConfigurationManager.OnMenusChanged += BuildMenus;
        }

        private void Update()
        {
            if (menuOpen)
            {
                CheckMenuOpen();
                return;
            }

            if (!cfgKey.WasPeformed())
                return;

            OpenMenu();
        }

        private void CheckMenuOpen()
        {
            if (menus == null)
                return;

            for(int i=0;i< menus.Length;i++)
            {
                if (menus[i] == null)
                    continue;

                if (menus[i].activeInHierarchy)
                    return;
            }

            //No menus are open so unpause.
            Unpause();
        }

        private void DestroyPages()
        {
            ConfigurationPage[] pages = pageManifest.Values.ToArray();

            for (int i=0;i<pages.Length;i++)
            {
                ConfigurationPage page = pages[i];
                Destroy(page.gameObject);
            }

            pageManifest.Clear();
            menuBuilt = false;
        }

        private void BuildMenus(ConfigBuilder[] menus)
        {
            DestroyPages();

            Debug.Log($"Building Configgy Menus {menus.Length}");

            NewPage((mainPage) =>
            {
                rootPage = mainPage;
                mainPage.SetHeader("Configuration");
                mainPage.Close();

                pageManifest.Add("", rootPage);
            });

            foreach(var menu in menus.OrderBy(x => x.OwnerDisplayName))
            {
                BuildMenu(menu.GetConfigElements());
            }
        }

        private void BuildMenu(IConfigElement[] configElements)
        {
            foreach (IConfigElement configElement in configElements.OrderBy(x=>x.GetDescriptor().Path))
            {
                BuildElement(configElement);
            }

            foreach(KeyValuePair<string, ConfigurationPage> page in pageManifest)
            {
                page.Value.RebuildPage();
            }

            menuBuilt = true;
        }

        private void BuildMenuTreeFromPath(string fullPath)
        {
            string[] path = fullPath.Split('/');

            if (path.Length > 0)
            {
                if (path[0] == "Configgy")
                {
                    for(int i = 0; i < path.Length; i++)
                    {
                        Debug.Log($"PATH{i}:{path[i]}");
                    }
                }
            }

            string currentPathAddress = "";

            ConfigurationPage previousPage = null;

            for (int i = 0; i < path.Length; i++)
            {
                currentPathAddress += path[i];

                if (!pageManifest.ContainsKey(currentPathAddress))
                {
                    if (previousPage == null)
                        previousPage = rootPage;

                    NewPage((page) =>
                    {
                        //Have to create a new reference because previousPage changes and it causes issues when the button is pressed.
                        ConfigurationPage closablePage = previousPage;

                        //Add button to parent page to access subpage
                        ConfigButton openSubPageButton = new ConfigButton(() =>
                        {
                            closablePage.Close();
                            page.Open();
                        }, path[i]);

                        closablePage.AddElement(openSubPageButton);

                        //Configure page
                        page.SetHeader(path[i]);
                        page.SetParent(closablePage);
                        page.gameObject.name = currentPathAddress;
                        page.SetFooter(currentPathAddress);
                        page.Close();

                        previousPage = page;
                        pageManifest.Add(currentPathAddress, page);
                    });
                }else
                {
                    previousPage = pageManifest[currentPathAddress];
                }
                
                currentPathAddress += "/";
            }
        }
        
        private void BuildElement(IConfigElement element)
        {
            ConfiggableAttribute descriptor = element.GetDescriptor();
            
            string path = "";

            if (descriptor != null && descriptor.Owner != null)
            {
                path = $"{descriptor.Owner.OwnerDisplayName}";
                if(!string.IsNullOrEmpty(descriptor.Path))
                    path += $"/{descriptor.Path}";
            }else
            {
                path = "/Other";
            }

            BuildMenuTreeFromPath(path);

            pageManifest[path].AddElement(element);
        }

        private void NewPage(Action<ConfigurationPage> onInstance)
        {
            GameObject newPage = GameObject.Instantiate(PluginAssets.ConfigurationPage, contentbody);
            ConfigurationPage page = newPage.GetComponent<ConfigurationPage>();
            newPage.AddComponent<BehaviourRelay>().OnOnEnable += (g) => lastOpenPage = page;
            onInstance?.Invoke(page);
        }

        

        public void OpenMenu()
        {
            if (menuOpen)
                return;

            menus = transform.GetChildren().Select(x=>x.gameObject).ToArray();
         
            GameState configState = new GameState("cfg_menu", menus);
            
            configState.cursorLock = LockMode.Unlock;
            configState.playerInputLock = LockMode.Lock;
            configState.cameraInputLock = LockMode.Lock;
            configState.priority = 20;

            OptionsManager.Instance.paused = true;
            Time.timeScale = 0f;
            GameStateManager.Instance.RegisterState(configState);

            if (lastOpenPage != null)
                lastOpenPage.Open();
            else
                rootPage.Open();
            
            menuOpen = true;
        }

        public void CloseMenu()
        {
            if (!menuOpen)
                return;

            menus = transform.GetChildren().Select(x => x.gameObject).ToArray();
            for(int i = 0; i < menus.Length; i++)
            {
                menus[i].SetActive(false);
            }
        }


        public static void Open()
        {
            onGlobalOpen?.Invoke();
        }

        public static void Close()
        {
            onGlobalClose?.Invoke();
        }

        private void Unpause()
        {
            GameStateManager.Instance.PopState("cfg_menu");
            OptionsManager.Instance.paused = false;
            Time.timeScale = 1f;
            menuOpen = false;
        }

        private void OnDestroy()
        {
            ConfigurationManager.OnMenusChanged -= BuildMenus;
            onGlobalOpen -= OpenMenu;
            onGlobalClose -= CloseMenu;

        }
    }
}
