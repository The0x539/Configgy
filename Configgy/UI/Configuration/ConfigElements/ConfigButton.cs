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

        public void BuildElement(RectTransform rect)
        {
            DynUI.Frame(rect, (panel) =>
            {
                DynUI.Button(panel.RectTransform, (b) =>
                {
                    b.GetComponentInChildren<Text>().text = GetLabel();
                    RectTransform buttonTf = b.GetComponent<RectTransform>();
                    DynUI.Layout.FillParent(buttonTf);
                    b.onClick.AddListener(() => { OnPress?.Invoke(); });
                });
            });
        }

        public void OnMenuOpen() { }
        public void OnMenuClose() { }
        public void BindConfig(ConfigBuilder configBuilder) { }
    }
}
