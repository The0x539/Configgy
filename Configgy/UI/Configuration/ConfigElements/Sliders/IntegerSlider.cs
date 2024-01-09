using BepInEx.Configuration;
using UnityEngine.UI;

namespace Configgy
{
    public class IntegerSlider : ConfigSlider<int>
    {
        public IntegerSlider(ConfigEntry<int> config, int min, int max) : base(config, min, max) { }

        protected override void OnConfigUpdate(int value)
        {
            slider.value = value;
            base.OnConfigUpdate(value);
        }

        protected override void SetValueFromSlider(float value)
        {
            SetConfigValueWithoutNotify((int)value);
        }

        protected override void InitializeSlider(Slider slider)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = config.Value;
            slider.wholeNumbers = true;
        }
    }
}
