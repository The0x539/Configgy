using Configgy.Assets;
using Configgy.UI;
using HarmonyLib;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

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
            InstanceOpenConfigButton(CanvasRect);
            InstanceModalDialogueManager(CanvasRect);
        }

        private static void InstanceModalDialogueManager(RectTransform rect)
        {
            GameObject modalDialogueManagerObject = GameObject.Instantiate(PluginAssets.ModalDialogueManager, rect);
        }

        private static void InstanceOpenConfigButton(RectTransform rect)
        {
            Transform pausemenu = rect.GetChildren().Where(x => x.name == "PauseMenu").FirstOrDefault();
            RectTransform pauseMenuRect = pausemenu.GetComponent<RectTransform>();

            Button optionMenuButton = pausemenu.GetComponentsInChildren<Button>().Where(x => x.name == "Options").FirstOrDefault();
            RectTransform optionButtonRect = optionMenuButton.GetComponent<RectTransform>();

            float buttonHeight = optionButtonRect.sizeDelta.y;
            Vector2 optionButtonPosition = optionButtonRect.anchoredPosition;

            DynUI.ImageButton(pauseMenuRect, (b,i) =>
            {
                RectTransform buttonRect = b.GetComponent<RectTransform>();
                buttonRect.SetAnchors(0.5f,0.5f,0.5f,0.5f);
                buttonRect.sizeDelta = new Vector2(buttonHeight, buttonHeight);
                optionButtonPosition.x += (optionButtonRect.sizeDelta.x / 2f) + (buttonHeight/2f) + 2f;
                buttonRect.anchoredPosition = optionButtonPosition;
                b.onClick.AddListener(() =>
                {
                    pauseMenuRect.gameObject.SetActive(false);
                    OptionsManager.Instance.UnPause();
                });
                b.onClick.AddListener(ConfigurationMenu.Open);
            });
        }
    }
}
