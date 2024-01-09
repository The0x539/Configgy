using BepInEx.Configuration;
using Configgy.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigDropdown<T> : ConfigValueElement<int>
    {
        public readonly string[] names;
        public readonly T[] values;
        protected Dropdown dropdown;

        public ConfigDropdown(ConfigEntry<int> entry, T[] values, string[] names = null) : base(entry)
        {
            if (names == null)
            {
                names = values.Select(x => x.ToString()).ToArray();
            }
            else if (names.Length != values.Length)
            {
                throw new ArgumentException("If names are provided, there must be one name for every value", nameof(names));
            }

            this.names = names;
            this.values = values;
        }

        protected override void OnConfigUpdate(int value)
        {
            dropdown.SetValueWithoutNotify(value);
        }

        protected override void BuildElement(RectTransform rect)
        {
            DynUI.Dropdown(rect, (dropdown) =>
            {
                dropdown.value = config.Value;
                dropdown.onValueChanged.AddListener((v) => SetValueFromDropdown(dropdown, v));
                dropdown.AddOptions(names.ToList());
                this.dropdown = dropdown;
            });
        }

        private void SetValueFromDropdown(Dropdown origin, int newValue)
        {
            if (origin != dropdown) return;
            SetConfigValueWithoutNotify(newValue);
        }
    }
}
