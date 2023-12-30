using Configgable.Assets;
using UnityEngine;

namespace Configgy.Assets
{

    public static class PluginAssets
    {
        private static AssetLoader assetLoader;
        private static bool initialized;
        public static void Initialize()
        {
            if (initialized)
                return;

            assetLoader = new AssetLoader(Properties.Resources.Configgy);
            LoadAssets();
            initialized = true;
        }

        private static void LoadAssets()
        {
            FramePrefab = assetLoader.LoadAsset<GameObject>("UI_Frame");
            ButtonPrefab = assetLoader.LoadAsset<GameObject>("UI_Button");
            ImageButtonPrefab = assetLoader.LoadAsset<GameObject>("UI_Button_Image");
            SliderPrefab = assetLoader.LoadAsset<GameObject>("UI_Slider");
            ScrollRectPrefab = assetLoader.LoadAsset<GameObject>("UI_ScrollRect");
            TextPrefab = assetLoader.LoadAsset<GameObject>("UI_Text");
            InputFieldPrefab = assetLoader.LoadAsset<GameObject>("UI_InputField");
            TogglePrefab = assetLoader.LoadAsset<GameObject>("UI_Toggle");
            DropdownPrefab = assetLoader.LoadAsset<GameObject>("UI_Dropdown");
            ConfigurationPage = assetLoader.LoadAsset<GameObject>("ConfigurationPage");
            ConfigurationMenu = assetLoader.LoadAsset<GameObject>("ConfigurationMenu");
            ModalDialogueManager = assetLoader.LoadAsset<GameObject>("ModalDialogueManager");
        }

        public static GameObject ConfigurationPage { get; private set; }
        public static GameObject ConfigurationMenu { get; private set; }
        public static GameObject FramePrefab { get; private set; }
        public static GameObject ButtonPrefab { get; private set; }
        public static GameObject ImageButtonPrefab { get; private set; }
        public static GameObject SliderPrefab { get; private set; }
        public static GameObject ScrollRectPrefab { get; private set; }
        public static GameObject TextPrefab { get; private set; }
        public static GameObject InputFieldPrefab { get; private set; }
        public static GameObject TogglePrefab { get; private set; }
        public static GameObject DropdownPrefab { get; private set; }
        public static GameObject ModalDialogueManager { get; private set; }


    }
}
