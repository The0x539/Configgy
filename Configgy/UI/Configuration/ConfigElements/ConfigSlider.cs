using BepInEx.Configuration;
using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public abstract class ConfigSlider<T> : ConfigValueElement<T>
    {
        protected readonly T min, max;
        protected Slider slider;
        protected Text outputText;

        public ConfigSlider(ConfigEntry<T> entry, T min, T max) : base(entry)
        {
            this.min = min;
            this.max = max;
        }

        protected override void OnConfigUpdate(T value)
        {
            outputText.text = value.ToString();
        }

        protected abstract void SetValueFromSlider(float value);
        private void SetValueFromSlider(Slider origin, float sliderValue)
        {
            if (origin != slider)
                return;

            SetValueFromSlider(sliderValue);
        }

        protected override void BuildElement(RectTransform rect)
        {
            DynUI.Label(rect, (text) =>
            {
                text.text = config.Value.ToString();
                outputText = text;
            });
            DynUI.Slider(rect, (slider) =>
            {
                slider.onValueChanged.AddListener((v) => SetValueFromSlider(slider, v));
                InitializeSlider(slider);
                this.slider = slider;
            });
        }

        protected abstract void InitializeSlider(Slider slider);
    }
}
