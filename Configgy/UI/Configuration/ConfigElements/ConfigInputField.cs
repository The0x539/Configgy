using BepInEx.Configuration;
using Configgy.UI;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ConfigInputField<T> : ConfigValueElement<T>
    {
        protected InputField textbox;

        public ConfigInputField(ConfigEntry<T> entry) : base(entry) { }

        protected override void OnConfigUpdate(T value)
        {
            textbox.SetTextWithoutNotify(value.ToString());
        }

        protected override void BuildElement(RectTransform rect)
        {
            DynUI.InputField(rect, (textbox) =>
            {
                textbox.text = StringifyValue(config.Value);
                textbox.onEndEdit.AddListener((s) => SetValueFromString(textbox, s));
                this.textbox = textbox;
            });
        }

        protected virtual bool ParseInput(string text, out T value)
        {
            // TODO: evaluate this vs. bepinex's toml-y converter system
            System.ComponentModel.TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            object erasedValue = converter.ConvertFromString(text);
            if (erasedValue == null)
            {
                value = default;
                return false;
            }
            else
            {
                value = (T)erasedValue;
                return true;
            }
        }

        protected virtual bool ValidateValue(T value)
        {
            return true;
        }

        protected virtual string StringifyValue(T value)
        {
            return value.ToString();
        }

        private void SetValueFromString(InputField origin, string input)
        {
            if (origin != textbox) //prevent old non-null instance from calling this method.
                return;

            bool success = false;
            T newValue;

            try
            {
                if (!ParseInput(input, out newValue))
                {
                    if (ValidateValue(newValue))
                    {
                        success = true;
                    }
                    else
                    {
                        Debug.LogError("Value validation failure. Rejected.");
                    }
                }
                else
                {
                    Debug.LogError("Syntax for field invalid! Conversion failed!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                newValue = default;
            }

            if (success)
            {
                SetConfigValueWithoutNotify(newValue);
            }
            else
            {
                textbox.SetTextWithoutNotify(StringifyValue(newValue));
            }
        }
    }
}
