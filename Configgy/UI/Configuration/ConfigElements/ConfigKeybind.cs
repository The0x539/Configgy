using BepInEx.Configuration;
using Configgy.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigKeybind : ConfigValueElement<KeyCode>
    {
        private Text keybindText;
        private Button keybindButton;
        private ConfigurationPage page;
        public bool IsBeingRebound { get; private set; }

        public ConfigKeybind(ConfigEntry<KeyCode> entry) : base(entry) { }

        protected override void OnConfigUpdate(KeyCode value)
        {
            keybindText.text = value.ToString();
        }

        protected override void BuildElement(RectTransform rect) {
            IsBeingRebound = false;
            page = rect.GetComponentInParent<ConfigurationPage>();
            DynUI.Button(rect, (button) =>
            {
                button.onClick.AddListener(StartRebinding);
                keybindText = button.GetComponentInChildren<Text>();
                keybindText.text = config.Value.ToString();
                keybindButton = button;
            });
        }

        private void StartRebinding()
        {
            page ??= keybindButton.GetComponentInParent<ConfigurationPage>();
            if (page == null)
            {
                Debug.LogError("Page could not be found?");
                return;
            }

            keybindText.text = "<color=orange>???</color>";
            keybindButton.interactable = false;

            IsBeingRebound = true;

            page.preventClosing = true;
            page.StartCoroutine(RebindProcess());
        }

        private void FinishRebind(KeyCode keycode)
        {
            keybindButton.interactable = true;
            page.preventClosing = false;
            IsBeingRebound = false;
            config.Value = keycode;
        }

        private IEnumerator RebindProcess()
        {
            float timer = 10f;
            IsBeingRebound = true;
            KeyCode currentKeyCode = config.Value;
            int counter = 0;
            yield return new WaitForSecondsRealtime(0.18f);

            while (IsBeingRebound && timer > 0f)
            {
                yield return null;
                timer -= Time.deltaTime;
                Event current = Event.current;

                if (current.type is not (EventType.KeyDown or EventType.KeyUp))
                {
                    continue;
                }

                switch (current.keyCode)
                {
                    case KeyCode.Escape:
                        FinishRebind(KeyCode.None);
                        yield break;
                    default:
                        FinishRebind(current.keyCode);
                        yield break;
                    case KeyCode.None:
                        break;
                }

                counter++;
            }

            FinishRebind(currentKeyCode);
        }
    }
}
