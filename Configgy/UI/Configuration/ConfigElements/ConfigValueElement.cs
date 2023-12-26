using System;
using UnityEngine;


namespace Configgy
{
    public abstract class ConfigValueElement<T> : IConfigElement
    {
        public T DefaultValue { get; }

        protected T? value;

        public Action<T> OnValueChanged;

        protected ConfiggableAttribute descriptor;

        protected ConfigBuilder config;

        private bool initialized => descriptor != null;
        protected bool firstLoadDone = false;

        public bool IsDirty { get; protected set; }

        public T Value
        {
            get
            {
                return GetValue();
            }
        }

        public ConfigValueElement(T defaultValue)
        {
            DefaultValue = defaultValue;
        }


        public void LoadValue()
        {
            if (!initialized)
                return;

            LoadValueCore();
            firstLoadDone = true; //just to be safe.
        }

        protected virtual void LoadValueCore()
        {
            //Get value from data manager.
            //This should probably be changed to something more reliable and not static.

            firstLoadDone = true; //nullable values apparently can just randomly have values so this annoying bool is needed

            if (config.TryGetValueAtAddress<T>(descriptor.SerializationAddress, out T value))
            {
                try
                {
                    SetValue(value);
                    return;
                } catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            ResetValue();
            //SaveValue(); TODO idk check this.
        }


        public void SaveValue()
        {
            SaveValueCore();
        }

        protected virtual void SaveValueCore()
        {
            object obj = GetValue();
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }


        public T GetValue()
        {
            return GetValueCore();
        }

        protected virtual T GetValueCore()
        {
            if (value == null || !firstLoadDone)
            {
                LoadValue();
            }
            
            return value;
        }

        public void SetValue(T value)
        {
            SetValueCore(value);
            IsDirty = true;
        }

        protected virtual void SetValueCore(T value)
        {
            this.value = value;
            OnValueChanged?.Invoke(value);
        }

        public void ResetValue()
        {
            ResetValueCore();
        }

        protected virtual void ResetValueCore()
        {
            SetValue(DefaultValue);
        }

        public void BindDescriptor(ConfiggableAttribute configgable)
        {
            this.descriptor = configgable;
        }

        public ConfiggableAttribute GetDescriptor()
        {
            return descriptor;
        }

        public void BuildElement(RectTransform rect)
        {
            if (!initialized)
                return;

            BuildElementCore(descriptor, rect);
        }

        protected abstract void BuildElementCore(ConfiggableAttribute descriptor, RectTransform rect);

        public override string ToString()
        {
            return GetValue().ToString();
        }

        public void RefreshElementValue()
        {
            RefreshElementValueCore();
        }

        protected abstract void RefreshElementValueCore();

        public void OnMenuOpen()
        {
            RefreshElementValue();
        }

        public void OnMenuClose()
        {
            if(IsDirty)
                SaveValue();
        }

        public void BindConfig(ConfigBuilder config)
        {
            this.config = config;
        }
    }
}
