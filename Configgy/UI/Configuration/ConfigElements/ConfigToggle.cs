using BepInEx.Configuration;
using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigToggle : ConfigValueElement<bool>
    {
        protected Toggle checkbox;

        public ConfigToggle(ConfigEntry<bool> entry) : base(entry) { }

        protected override void BuildElement(RectTransform rect) {
            DynUI.Toggle(rect, checkbox =>
            {
                checkbox.isOn = config.Value;
                checkbox.onValueChanged.AddListener(SetConfigValueWithoutNotify);
                this.checkbox = checkbox;
            });
        }

        protected override void OnConfigUpdate(bool value)
        {
            checkbox?.SetIsOnWithoutNotify(value);
        }
    }
}
