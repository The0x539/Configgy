using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.Assertions;


namespace Configgy
{
    public abstract class ConfigValueElement : IConfigElement
    {
        protected ConfiggableAttribute descriptor;
        protected ConfigBuilder config;

        private bool initialized => descriptor != null;
        protected bool firstLoadDone = false;
        public bool IsDirty { get; protected set; }

        protected abstract void BuildElementCore(RectTransform rect);

        internal abstract void BindField(FieldInfo field);

        protected abstract void LoadValueCore();
        protected abstract void SaveValueCore();
        protected abstract void ResetValueCore();
        protected abstract void RefreshElementValueCore();

        public void LoadValue()
        {
            if (!initialized)
                return;

            LoadValueCore();
            firstLoadDone = true; //just to be safe.
        }

        public void SaveValue() => SaveValueCore();
        public void ResetValue() => ResetValueCore();
        public void RefreshElementValue() => RefreshElementValueCore();

        #region Implement IConfigElement

        public void BuildElement(RectTransform rect)
        {
            if (!initialized)
                return;

            BuildElementCore(rect);
        }

        public void OnMenuOpen()
        {
            RefreshElementValue();
        }

        public void OnMenuClose()
        {
            if (IsDirty)
                SaveValue();
        }

        void IConfigElement.BindDescriptor(ConfiggableAttribute configgable) => this.descriptor = configgable;
        internal ConfiggableAttribute GetDescriptor() => descriptor;
        ConfiggableAttribute IConfigElement.GetDescriptor() => descriptor;
        void IConfigElement.BindConfig(ConfigBuilder config) => this.config = config;

        #endregion
    }

    public abstract class ConfigValueElement<T> : ConfigValueElement
    {
        public T DefaultValue { get; }

        protected T? value;

        public Action<T> OnValueChanged;

        public ConfigValueElement(T defaultValue)
        {
            DefaultValue = defaultValue;
        }

        protected override void LoadValueCore()
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
            object obj = GetValue();
            config.SetValueAtAddress(descriptor.SerializationAddress, obj);
            config.SaveDeferred();
            IsDirty = false;
        }

        public T Value => GetValue();

        public T GetValue() => GetValueCore();

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

        protected override void ResetValueCore()
        {
            SetValue(DefaultValue);
        }

        public override string ToString() => GetValue().ToString();

        internal sealed override void BindField(FieldInfo field)
        {
            Assert.AreEqual(expected: typeof(T), actual: field.FieldType);
            OnValueChanged += v => field.SetValue(null, v);
        }
    }
}
