using BepInEx.Configuration;
using System;

namespace Configgy.Configuration.AutoGeneration
{
    //Yes it just removes the serialization. It sucks. It works. I don't care.
    internal class BepinInputField<T> : ConfigInputField<T>
    {
        protected ConfigEntry<T> entry;

        public BepinInputField(T defaultValue, ConfigEntry<T> entry, Func<T, bool> inputValidator = null, Func<string, (bool, T)> typeConverter = null) : base(defaultValue, inputValidator, typeConverter)
        {
            this.entry = entry;
        }

        protected override void LoadValueCore()
        {
            firstLoadDone = true;
            //Do nothing.
        }

        protected override T GetValueCore()
        {
            return entry.Value;
        }

        protected override void SetValueCore(T value)
        {
            entry.Value = value;
            OnValueChanged?.Invoke(value);
        }

        protected override void SaveValueCore()
        {
            //do nothing.
            IsDirty = false;
        }
    }
}
