﻿using Configgy.UI;
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

        private Configgable descriptor;

        public void BindDescriptor(Configgable descriptor)
        {
            this.descriptor = descriptor;
        }

        public Configgable GetDescriptor()
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
    }
}