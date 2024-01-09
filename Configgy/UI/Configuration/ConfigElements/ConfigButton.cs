using BepInEx.Configuration;
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

        public ConfigButton(Action onPress, string label = null, ConfigElementMetadata metadata = null)
        {
            this.OnPress = onPress;
            this.label = label;
            Metadata = metadata ?? new();
        }

        public ConfigBuilder Parent { get; set; }

        public ConfigDefinition Definition { get; } = new("", ""); // dummy value
        public ConfigElementMetadata Metadata { get; }

        private string GetLabel()
        {
            if (!string.IsNullOrEmpty(label))
                return label;

            return OnPress.Method.Name;
        }

        public void BuildElement(RectTransform rect)
        {
            DynUI.Frame(rect, (panel) =>
            {
                panel.RectTransform.sizeDelta = new Vector2(panel.RectTransform.sizeDelta.x, 55);

                DynUI.Button(panel.RectTransform, (b) =>
                {
                    b.GetComponentInChildren<Text>().text = GetLabel();
                    RectTransform buttonTf = b.GetComponent<RectTransform>();
                    DynUI.Layout.FillParent(buttonTf);
                    b.onClick.AddListener(() => { OnPress?.Invoke(); });
                });
            });
        }
    }
}
