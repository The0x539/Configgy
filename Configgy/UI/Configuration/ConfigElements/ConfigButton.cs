using Configgy.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigButton : IConfigElement
    {
        public Action OnPress;
        private string label;

        private Button instancedButton;
        private Text buttonText;

        public Button GetButton() => instancedButton;
        public Text GetButtonLabel() => buttonText;

        public ConfigButton(Action onPress, string label = null)
        {
            this.OnPress = onPress;
            this.label = label;
        }

        private ConfiggableAttribute descriptor;

        public void BindDescriptor(ConfiggableAttribute descriptor)
        {
            this.descriptor = descriptor;
        }

        public ConfiggableAttribute GetDescriptor()
        {
            return descriptor;
        }

        public string GetLabel()
        {
            if (!string.IsNullOrEmpty(label))
                return label;

            if (descriptor != null)
                return descriptor.DisplayName;

            return OnPress.Method.Name;
        }

        private void OnButtonPressed()
        {
            OnPress?.Invoke();
        }

        public void SetLabel(string label)
        {
            this.label = label;

            if (buttonText != null)
            {
                buttonText.text = GetLabel();
            }
        }

        public void BuildElement(RectTransform rect)
        {
            DynUI.Frame(rect, (panel) =>
            {
                panel.RectTransform.sizeDelta = new Vector2(panel.RectTransform.sizeDelta.x, 55);

                DynUI.Button(panel.RectTransform, (b) =>
                {
                    instancedButton = b;
                    Text text = b.GetComponentInChildren<Text>();
                    buttonText = text;
                    text.text = GetLabel();
                    RectTransform buttonTf = b.GetComponent<RectTransform>();
                    DynUI.Layout.FillParent(buttonTf);
                    b.onClick.AddListener(OnButtonPressed);
                });
            });
        }

        public void OnMenuOpen() { }
        public void OnMenuClose() { }
        public void BindConfig(ConfigBuilder configBuilder) { }
    }
}
