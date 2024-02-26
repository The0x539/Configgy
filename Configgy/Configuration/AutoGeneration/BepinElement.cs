using BepInEx.Configuration;

using System.Reflection;

using UnityEngine;
using UnityEngine.Assertions;

namespace Configgy.Configuration.AutoGeneration
{
    internal class BepinElement<T> : ConfigValueElement<T>
    {
        private readonly ConfigEntry<T> entry;
        private readonly ConfigValueElement<T> element;

        public BepinElement(ConfigEntry<T> entry, ConfigValueElement<T> element) : base(entry.GetDefault())
        {
            element.SetValue(entry.Value);
            this.entry = entry;
            this.element = element;
        }

        protected override void BuildElementCore(RectTransform rect) => element.BuildElement(rect);
        protected override void RefreshElementValueCore() => element.RefreshElementValue();

        protected override void LoadValueCore()
        {
            // do nothing
            firstLoadDone = true; // inherited baggage
        }

        protected override void SaveValueCore()
        {
            DirtyConfigFiles.Save(entry.ConfigFile);
            IsDirty = false; // inherited baggage
        }

        public override T DefaultValue => entry.GetDefault();

        protected override T GetValueCore() => entry.Value;

        protected override void SetValueCore(T value)
        {
            entry.Value = value;
            OnValueChanged?.Invoke(value);
            element.SetValue(value); // kinda inherited baggage
            DirtyConfigFiles.Mark(entry.ConfigFile);
        }
    }

    internal static class BepinElement
    {
        // This function mostly exists because it's more convenient to locate with reflection than the actual constructor.
        public static BepinElement<T> Wrap<T>(ConfigEntry<T> entry, ConfigValueElement<T> element)
        {
            return new BepinElement<T>(entry, element);
        }

        // A heinous bridge from reflection to generics.
        private static readonly MethodInfo unboundWrapMethod = typeof(ConfigBuilder).GetMethod(nameof(Wrap));
        public static ConfigValueElement WrapUntyped(ConfigEntryBase entry, ConfigValueElement element)
        {
            Assert.AreEqual(element.ConfigValueType, entry.SettingType);
            MethodInfo boundMethod = unboundWrapMethod.MakeGenericMethod(entry.SettingType);
            return (ConfigValueElement)boundMethod.Invoke(null, [entry, element]);
        }
    }
}
