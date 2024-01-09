using UnityEngine;
using BepInEx.Configuration;
using System;
using Configgy.UI;
using System.Linq;

namespace Configgy
{
    public abstract class ConfigValueElement<T> : IConfigElement
    {
        protected ConfigEntry<T> config;
        //private bool initialized = false;

        public ConfigValueElement(ConfigEntry<T> entry)
        {
            this.config = entry;
            Metadata = entry.Description.Tags.OfType<ConfigElementMetadata>().FirstOrDefault() ?? new();
            entry.SettingChanged += OnConfigUpdateCore;
        }

        public ConfigBuilder Parent { get; set; }

        public ConfigDefinition Definition => config.Definition;
        public ConfigDescription Description => config.Description;
        public ConfigElementMetadata Metadata { get; }

        protected abstract void OnConfigUpdate(T value);
        private void OnConfigUpdateCore(object sender, EventArgs args) => OnConfigUpdate(config.Value);

        protected void SetConfigValueWithoutNotify(T value)
        {
            config.SettingChanged -= OnConfigUpdateCore;
            config.Value = value;
            config.SettingChanged += OnConfigUpdateCore;
        }

        void IConfigElement.BuildElement(RectTransform rect)
        {
            //if (initialized) return;
            DynUI.ConfigUI.CreateElementSlot(rect, this, BuildElement);
            //initialized = true;
        }

        protected abstract void BuildElement(RectTransform rect);

        public virtual void ResetValue()
        {
            config.Value = (T)config.DefaultValue;
        }
    }
}
