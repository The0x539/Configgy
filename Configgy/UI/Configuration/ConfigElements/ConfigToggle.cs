using BepInEx.Configuration;
using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigToggle : ConfigValueElement<bool>
    {
        protected Toggle toggle;

        public ConfigToggle(ConfigEntry<bool> entry) : base(entry) { }

        protected override void BuildElement(RectTransform rect) {
            DynUI.Toggle(rect, toggle =>
            {
                toggle.isOn = config.Value;
                toggle.onValueChanged.AddListener((v) => SetValueFromToggle(toggle, v));
                this.toggle = toggle;
            });
        }

        protected void SetValueFromToggle(Toggle origin, bool value)
        {
            if (origin != toggle) return;
            SetConfigValueWithoutNotify(value);
        }

        protected override void OnConfigUpdate(bool value)
        {
            toggle?.SetIsOnWithoutNotify(value);
        }
    }
}
