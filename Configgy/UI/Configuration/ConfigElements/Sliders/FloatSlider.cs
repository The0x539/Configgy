using BepInEx.Configuration;
using UnityEngine.UI;

namespace Configgy
{
    public class FloatSlider : ConfigSlider<float>
    {
        public FloatSlider(ConfigEntry<float> config, float min, float max) : base(config, min, max) { }

        protected override void OnConfigUpdate(float value)
        {
            slider.value = value;
            base.OnConfigUpdate(value);
        }

        protected override void SetValueFromSlider(float value)
        {
            SetConfigValueWithoutNotify(value);
        }

        protected override void InitializeSlider(Slider slider)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = config.Value;
            slider.wholeNumbers = false;
        }
    }
}
