using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace Configgy
{
    public class ConfigBuilder
    {
        private readonly List<IConfigElement> configElements = new();
        public IReadOnlyList<IConfigElement> ConfigElements => configElements;

        public string GUID { get; private set; }
        public string OwnerDisplayName { get; private set; }

        public ConfigBuilder(string name, string guid)
        {
            GUID = guid;
            OwnerDisplayName = name;
        }

        public ConfigBuilder AddFile(ConfigFile configFile)
        {
            foreach (var pair in configFile)
            {
                AddElement(GuessElementFromUntypedEntry(pair.Value));
            }
            return this;
        }

        public ConfigBuilder AddElement(IConfigElement element)
        {
            if (element != null)
            {
                element.Parent = this;
                configElements.Add(element);
            }
            return this;
        }

        public void Build()
        {
            ConfigurationManager.RegisterConfiguraitonMenu(this);
        }

        private static IConfigElement GuessElementFromUntypedEntry(ConfigEntryBase entry)
        {
            if (entry is ConfigEntry<int> intEntry)
            {
                if (entry.Description.AcceptableValues is AcceptableValueRange<int> range)
                {
                    return new IntegerSlider(intEntry, range.MinValue, range.MaxValue);
                }
                else
                {
                    return new ConfigInputField<int>(intEntry);
                }
            }
            else if (entry is ConfigEntry<float> floatEntry)
            {
                if (entry.Description.AcceptableValues is AcceptableValueRange<float> range)
                {
                    return new FloatSlider(floatEntry, range.MinValue, range.MaxValue);
                }
                else
                {
                    return new ConfigInputField<float>(floatEntry);
                }
            }
            else if (entry is ConfigEntry<KeyCode> keyEntry)
            {
                return new ConfigKeybind(keyEntry);
            }
            else if (entry is ConfigEntry<bool> boolEntry)
            {
                return new ConfigToggle(boolEntry);
            }
            else if (entry is ConfigEntry<Color> colorEntry)
            {
                return new ConfigColor(colorEntry);
            }

            return null;
        }
    }
}
