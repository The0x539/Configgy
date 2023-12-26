using System;
using UnityEngine;

namespace Configgy
{
    public class ConfigCustomElement : IConfigElement
    {
        private Action<ConfiggableAttribute, RectTransform> onBuild;

        private ConfiggableAttribute configgable;

        protected ConfigBuilder config;

        public Action OnMenuClosed;
        public Action OnMenuOpened;

        public ConfigCustomElement(Action<ConfiggableAttribute, RectTransform> onBuild)
        {
            this.onBuild = onBuild;
        }
        
        public void BindDescriptor(ConfiggableAttribute configgable)
        {
            this.configgable = configgable;
        }

        public void BuildElement(RectTransform rect)
        {
            onBuild?.Invoke(configgable, rect);
        }

        public ConfiggableAttribute GetDescriptor()
        {
            return configgable;
        }

        public void OnMenuOpen()
        {
            OnMenuOpened?.Invoke();
        }

        public void OnMenuClose()
        {
            OnMenuClosed?.Invoke();
        }

        public void BindConfig(ConfigBuilder configBuilder)
        {
            config = configBuilder;
        }
    }
}
