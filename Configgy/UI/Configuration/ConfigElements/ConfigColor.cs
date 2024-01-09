using BepInEx.Configuration;
using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigColor : ConfigValueElement<Color>
    {
        private readonly Slider[] sliders = new Slider[4];
        private InputField textbox;
        private Image colorDisplay;

        public ConfigColor(ConfigEntry<Color> config) : base(config) { }

        protected override void BuildElement(RectTransform rect)
        {
            DynUI.InputField(rect, (input) =>
            {
                input.text = ToHex(config.Value);
                input.onEndEdit.AddListener(SetValueFromText);
                textbox = input;
            });

            for(int i = 0; i < sliders.Length; i++)
            {
                int j = i;
                DynUI.Slider(rect, (slider) =>
                {
                    slider.minValue = 0;
                    slider.maxValue = 1;
                    slider.wholeNumbers = false;
                    slider.value = config.Value[j];
                    slider.onValueChanged.AddListener((v) => SetValueFromSlider(j, v));
                    sliders[j] = slider;
                });
            }

            DynUI.ImageButton(rect, (button, image) =>
            {
                Object.Destroy(button);
                image.sprite = null;
                image.color = config.Value;
                colorDisplay = image;
            });
        }

        private static string ToHex(Color color) => ColorUtility.ToHtmlStringRGBA(color);
        private static bool FromHex(string hex, out Color color) => ColorUtility.TryParseHtmlString(hex, out color);

        private void SetValueFromSlider(int index, float value)
        {
            var color = config.Value;
            color[index] = value;
            config.Value = color;
        }

        private void SetValueFromText(string s)
        {
            if (!s.StartsWith("#"))
            {
                s = $"#{s}";
            }

            if (FromHex(s, out Color color))
            {
                config.Value = color;
            }
            else
            {
                textbox?.SetTextWithoutNotify(ToHex(config.Value));
            }
        }

        protected override void OnConfigUpdate(Color value)
        {
            for(int i = 0; i < sliders.Length; i++)
            {
                sliders[i]?.SetValueWithoutNotify(value[i]);
            }

            textbox?.SetTextWithoutNotify(ToHex(value));

            if (colorDisplay != null)
            {
                colorDisplay.color = value;
            }
        }
    }
}
