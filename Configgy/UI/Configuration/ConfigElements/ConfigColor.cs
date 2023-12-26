using Configgy.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigColor : ConfigValueElement<Color>
    {
        public ConfigColor(Color defaultValue) : base(defaultValue)
        {
            OnValueChanged += (_) => RefreshElementValue();
            OnValueChanged += UpdateColorDisplayColor;
            serializedColor = new SerializedColor(defaultValue);
            sliders = new Slider[4];
        }

        private SerializedColor serializedColor;

        private Slider[] sliders;
        private Image colorDisplay;

        protected override void BuildElementCore(ConfiggableAttribute descriptor, RectTransform rect)
        {
            DynUI.ConfigUI.CreateElementSlot(rect, this, (r) =>
            {
                for(int i = 0; i < sliders.Length; i++)
                {
                    DynUI.Slider(r, (s) =>
                    {
                        SetSlider(s, i);
                    });
                }

                DynUI.ImageButton(r, (b, i) =>
                {
                    UnityEngine.Object.Destroy(b);
                    SetColorDisplay(i);
                });
            });
        }

        private void SetColorDisplay(Image image)
        {
            image.sprite = null;
            colorDisplay = image;
        }

        private void UpdateColorDisplayColor(Color color)
        {
            if (colorDisplay == null)
                return;

            colorDisplay.color = color;
        }

        protected override Color GetValueCore()
        {
            if (value == null || !firstLoadDone)
            {
                LoadValue();
            }

            return value;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true; //nullable values apparently can just randomly have values so this annoying bool is needed

            if (config.TryGetValueAtAddress<SerializedColor>(descriptor.SerializationAddress, out SerializedColor color))
            {
                try
                {
                    SetValue(color.ToColor());
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            ResetValue();
            //SaveValue(); TODO idk check this.
        }

        protected override void SaveValueCore()
        {
            object obj = serializedColor;
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }

        private void SetSlider(Slider slider, int index)
        {
            sliders[index] = slider;
            slider.onValueChanged.AddListener((v) => SetValueFromSlider(index, v));
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
        }

        private void SetValueFromSlider(int index, float value)
        {
            serializedColor[index] = value;
            SetValue(serializedColor.ToColor());
        }

        protected override void SetValueCore(Color value)
        {
            this.value = value;
            serializedColor = new SerializedColor(value);
            OnValueChanged?.Invoke(value);
        }

        protected override void RefreshElementValueCore()
        {
            if (sliders == null)
                return;

            for(int i=0;i<sliders.Length;i++)
            {
                if (sliders[i] == null)
                    return;

                sliders[i].SetValueWithoutNotify(serializedColor[i]);
            }

            UpdateColorDisplayColor(serializedColor.ToColor());
        }
    }

    [Serializable]
    public struct SerializedColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializedColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return r;
                    case 1:
                        return g;
                    case 2:
                        return b;
                    case 3:
                        return a;
                    default:
                        throw new IndexOutOfRangeException("Index is out of range of SerializedColor's values.");
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        r = value;
                        break;
                    case 1:
                        g = value;
                        break;
                    case 2:
                        b = value;
                        break;
                    case 3:
                        a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Index is out of range of SerializedColor's values.");
                }
            }
        }
    }
}
