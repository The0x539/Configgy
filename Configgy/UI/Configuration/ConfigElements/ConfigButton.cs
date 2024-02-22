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

        private Button buttonInstance;
        private Text buttonText;

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

        private string GetLabel()
        {
            if (!string.IsNullOrEmpty(label))
                return label;

            if (descriptor != null)
                return descriptor.DisplayName;

            return OnPress.Method.Name;
        }

        public void SetLabel(string label)
        {
            this.label = label;
            
            if (buttonText)
                buttonText.text = label;
        }

        public void SetAction(Action onPress)
        {
            this.OnPress = onPress;

            if (buttonInstance)
            {
                buttonInstance.onClick.RemoveAllListeners();
                buttonInstance.onClick.AddListener(OnPress.Invoke);
            }
        }

        public void BuildElement(RectTransform rect)
        {
            DynUI.Frame(rect, (panel) =>
            {
                panel.RectTransform.sizeDelta = new Vector2(panel.RectTransform.sizeDelta.x, 55);

                DynUI.Button(panel.RectTransform, (b) =>
                {
                    buttonInstance = b;
                    buttonText = b.GetComponentInChildren<Text>();
                    buttonText.text = GetLabel();

                    RectTransform buttonTf = b.GetComponent<RectTransform>();
                    DynUI.Layout.FillParent(buttonTf);
                    b.onClick.AddListener(() => { OnPress?.Invoke(); });
                });
            });
        }

        public virtual void OnMenuOpen() { }
        public virtual void OnMenuClose() { }
        public void BindConfig(ConfigBuilder configBuilder) { }
    }
}
