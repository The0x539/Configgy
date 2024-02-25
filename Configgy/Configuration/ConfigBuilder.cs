using BepInEx.Configuration;
using Configgy.Configuration.AutoGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Configgy
{
    /// <summary>
    /// A class that allows you to build a configuration menu from your assembly.
    /// </summary>
    public class ConfigBuilder
    {
        /// <summary>
        /// The GUID of the config. This is used to identify the config in the configuration menu and serialize it's data.
        /// </summary>
        public string GUID { get; }

        /// <summary>
        /// The display name of the config in the root of the configuration menu.
        /// </summary>
        public string DisplayName { get; }

        internal bool initialized { get; private set; }

        internal Assembly owner;
        internal List<IConfigElement> _configElements;

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">Your mod GUID ex:"JohnModder.ULTRAKILL.MyMod"</param>
        /// <param name="menuDisplayName">The display name for your config in the configuration menu.</param>
        public ConfigBuilder(string guid = null, string menuDisplayName = null)
        {
            this.owner = Assembly.GetCallingAssembly();
            this.GUID = (string.IsNullOrEmpty(guid) ? owner.GetName().Name : guid);
            this.DisplayName = (string.IsNullOrEmpty(menuDisplayName) ? GUID : menuDisplayName);
        }

        internal static HashSet<Type> environmentBuiltTypes = new HashSet<Type>();

        /// <summary>
        /// Builds provided all config elements within the provided types.
        /// </summary>
        /// <param name="types"></param>
        public void BuildTypes(params Type[] types)
        {
            foreach (var type in types)
            {
                BuildType(type);
            }
        }

        /// <summary>
        /// Builds all config elements defined in a specific type.
        /// </summary>
        /// <param name="type"></param>
        public void BuildType(Type type)
        {
            owner ??= Assembly.GetCallingAssembly();

            if (owner != type.Assembly)
                throw new Exception($"Configgy.ConfigBuilder:{GUID}: You can only build types originating from the assembly that owns the ConfigBuilder.");

            if (environmentBuiltTypes.Contains(type))
            {
                if (builtGlobal)
                    throw new Exception($"Configgy.ConfigBuilder:{GUID}: Type has already been built into an existing config. If this is a mistake and you called BuildAll, you must call BuildType before BuildAll.");
                else
                    throw new Exception($"Configgy.ConfigBuilder:{GUID}: Type has already been built into an existing config.");
            }

            _configElements ??= new List<IConfigElement>();
            ProcessType(type);
            BuildInternal();
        }

        private bool builtGlobal = false;

        /// <summary>
        /// Builds config elements from any un-built types' configgable fields and methods in your assembly.
        /// </summary>
        public void BuildAll()
        {
            //only build global once.
            if (builtGlobal)
                return;

            owner ??= Assembly.GetCallingAssembly();
            _configElements ??= new List<IConfigElement>();

            foreach (Type type in owner.GetTypes())
                ProcessType(type);

            builtGlobal = true;
            BuildInternal();
        }


        /// <summary>
        /// Builds your configuration menu and registers it with Configgy.
        /// </summary>
        [Obsolete($"Build is now obsolete. Use {nameof(ConfigBuilder)}.{nameof(BuildAll)} to build using your whole assembly.")]
        public void Build()
        {
            if (initialized)
                return;

            BuildAll();
        }

        internal void ProcessType(Type type)
        {
            //Possibly built by manual call. Ignore it.
            if (environmentBuiltTypes.Contains(type))
                return;

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                ProcessMethod(method);

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                ProcessField(field);

            environmentBuiltTypes.Add(type);
        }

        internal void BuildInternal()
        {
            if (initialized)
                return;

            _configElements ??= new List<IConfigElement>();
            CreateSaverObject();

            OnConfigElementsChanged += (v) => ConfigurationManager.SubMenuElementsChanged();
            OnConfigElementsChanged?.Invoke(_configElements.ToArray());

            initialized = true;
            ConfigurationManager.RegisterConfiguraitonMenu(this);
        }

        /// <summary>
        /// Rebuilds the config menu. Do not use in a hot path as this is resource intensive.
        /// </summary>
        public void Rebuild()
        {
            OnConfigElementsChanged?.Invoke(_configElements.ToArray());
        }

        /// <summary>
        /// Invoked when the menu is rebuilt.
        /// </summary>
        public event Action<IConfigElement[]> OnConfigElementsChanged;

        internal IConfigElement[] GetConfigElements()
        {
            return _configElements.ToArray();
        }

        private void ProcessMethod(MethodInfo method)
        {
            ConfiggableAttribute cfg = method.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

            if (method.ReturnType != typeof(void))
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register method {method.DeclaringType.Name}.{method.Name}. But it's return type is not void. Skipping!");
                return;
            }

            if (method.GetParameters().Length > 0)
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register method {method.DeclaringType.Name}.{method.Name}. But it has more than 0 parameters. Skipping!");
                return;
            }

            if (!method.IsStatic) //no instance!!
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register method {method.DeclaringType.Name}.{method.Name}. But it is not static. Skipping!");
                return;
            }

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}"); //This isnt needed, but who cares.

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(method.Name);

            RegisterMethodAsButton(cfg, method);
        }

        private void ProcessField(FieldInfo field)
        {
            ConfiggableAttribute cfg = field.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

            if (!field.IsStatic) //no instance!!
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register field {field.DeclaringType.Name}.{field.Name}. But it is not static. Skipping!");
                return;
            }

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{field.DeclaringType.Namespace}.{field.DeclaringType.Name}.{field.Name}");

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(field.Name);

            if (typeof(IConfigElement).IsAssignableFrom(field.FieldType))
            {
                IConfigElement cfgElement = (IConfigElement)field.GetValue(null);
                RegisterElementCore(cfg, cfgElement);
            }
            else
            {
                RegisterPrimitive(cfg, field);
            }
        }

        private void RegisterMethodAsButton(ConfiggableAttribute descriptor, MethodInfo method)
        {
            ConfigButton button = new ConfigButton(() =>
            {
                method.Invoke(null, null);
            });

            //Not really needed, but who cares.
            button.BindConfig(this);
            button.BindDescriptor(descriptor);
            RegisterElementCore(descriptor, button);
        }

        private void RegisterElementCore(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            configElement.BindDescriptor(descriptor);
            configElement.BindConfig(this);
            _configElements.Add(configElement);
        }

        /// <summary>
        /// Manually registers a config element with a descriptor. Call <see cref="ConfigBuilder.BuildWithoutScan"/> to rebuild the menu after adding elements.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="configElement"></param>
        public void RegisterElement(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            BuildInternal();
            RegisterElementCore(descriptor, configElement);
        }

        #region Element construction for BepInEx.Configuration support

        public void RegisterBepInExConfigEntry(ConfigEntryBase entry)
        {
            BuildInternal();

            //Use the display name for the parent of this entry.
            ConfiggableAttribute attribute = new ConfiggableAttribute(
                entry.Definition.Section,
                entry.Definition.Key,
                description: entry.Description.Description
                );

            attribute.SetOwner(this);

            IConfigElement configElement = entry switch
            {
                ConfigEntry<bool> e => new BepinToggle(e),
                ConfigEntry<float> e => BepinPrimitiveElement(e),
                ConfigEntry<int> e => BepinPrimitiveElement(e),
                ConfigEntry<string> e => BepinPrimitiveElement(e),
                ConfigEntry<uint> e => BepinPrimitiveElement(e),
                ConfigEntry<long> e => BepinPrimitiveElement(e),
                ConfigEntry<ulong> e => BepinPrimitiveElement(e),
                ConfigEntry<char> e => BepinPrimitiveElement(e),
                ConfigEntry<Color> e => new BepinColor(e),
                ConfigEntry<Vector3> e => new BepinVector3(e),
                ConfigEntry<Vector2> e => new BepinVector2(e),
                ConfigEntry<Quaternion> e => new BepinQuaternion(e),
                ConfigEntry<KeyCode> e => new BepinKeybind(e),
                ConfigEntry<KeyboardShortcut> e => ShortcutAsKeybind(e),
                _ => BepinUnsupportedType(entry)
            };

            if (configElement is null)
                return;

            RegisterElementCore(attribute, configElement);
        }

        private IConfigElement BepinUnsupportedType(ConfigEntryBase entry)
        {
            Debug.LogWarning($"Configgy.ConfigBuilder:{GUID}: failed to auto generate BepInEx ConfigEntry {entry.Definition.Section}.{entry.Definition.Key}. It's type ({entry.SettingType.Name}) is not supported.");
            return null;
        }

        private static IConfigElement BepinPrimitiveElement<T>(ConfigEntry<T> entry) where T : IEquatable<T>
        {
            return (entry?.Description?.AcceptableValues) switch
            {
                // acceptable value ranges for single-precision floats and 32-bit signed integers get sliders
                // TODO: make shit more generic so supporting long/uint/double wouldn't require defining more classes
                AcceptableValueRange<float> range => new BepinFloatSlider(entry as ConfigEntry<float>, range),
                AcceptableValueRange<int> range => new BepinIntegerSlider(entry as ConfigEntry<int>, range),

                // anything with an AcceptableValueList gets a dropdown
                // (as long as we managed to even call this method, which is a challenge for custom types)
                AcceptableValueList<T> list => new BepinDropdown<T>(entry, list),

                // fallback: if there's any sort of validator we can work with, use that
                AcceptableValueBase avb => new BepinInputField<T>(entry, val => avb.IsValid(val)),

                // fallback 2: billion-dollar-mistake boogaloo
                null => new BepinInputField<T>(entry),
            };
        }

        private IConfigElement ShortcutAsKeybind(ConfigEntry<KeyboardShortcut> entry)
        {
            if (entry.Value.Modifiers.Any())
            {
                Debug.LogWarning($"Configgy.ConfigBuilder:{GUID}: failed to auto generate BepInEx ConfigEntry {entry.Definition.Section}.{entry.Definition.Key}. Configgy does not support multi key keybinds. Removing the modifiers within the config file manually will allow this element to be generated as a single-key keybind.");
                return null;
            }

            return new BepinKeybind(entry);
        }

        #endregion

        #region Element construction for ConfiggableAttribute support

        private void RegisterPrimitive(ConfiggableAttribute descriptor, FieldInfo field)
        {
            ConfigValueElement element = MakeElementForField(field);
            if (element != null)
            {
                element.BindField(field);
                RegisterElementCore(descriptor, element);
            }
        }

        private ConfigValueElement MakeElementForField(FieldInfo field)
        {
            object baseValue = field.GetValue(null);

            if (field.FieldType.IsEnum)
            {
                return FieldEnumDropdownUntyped(baseValue);
            }

            if (field.GetCustomAttribute<RangeAttribute>() is RangeAttribute range)
            {
                if (baseValue is float f)
                {
                    float min = range.min, max = range.max;
                    float defaultValue = Mathf.Clamp(f, min, max);
                    return new FloatSlider(defaultValue, min, max);
                }
                else if (baseValue is int i)
                {
                    int min = (int)range.min, max = (int)range.max;
                    int defaultValue = Mathf.Clamp(i, min, max);
                    return new IntegerSlider(defaultValue, min, max);
                }
            }

            return baseValue switch
            {
                int v => new ConfigInputField<int>(v),
                float v => new ConfigInputField<float>(v),
                uint v => new ConfigInputField<uint>(v),
                long v => new ConfigInputField<long>(v),
                ulong v => new ConfigInputField<ulong>(v),
                string v => new ConfigInputField<string>(v),
                char v => new ConfigInputField<char>(v),
                bool v => new ConfigToggle(v),
                Vector2 v => new ConfigVector2(v),
                Vector3 v => new ConfigVector3(v),
                Quaternion v => new ConfigQuaternion(v),
                Color v => new ConfigColor(v),
                _ => FieldUnsupportedType(field),
            };
        }

        private ConfigValueElement FieldUnsupportedType(FieldInfo field)
        {
            Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register field {field.DeclaringType.Name}.{field.Name}. But it's type ({field.FieldType.Name}) is not supported. Skipping!");
            return null;
        }

        private static ConfigDropdown<T> FieldEnumDropdown<T>(T baseValue)
        {
            T[] values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            string[] names = Enum.GetNames(typeof(T));

            int defaultIndex = Array.FindIndex(values, v => baseValue.Equals(v));
            if (defaultIndex < 0)
                defaultIndex = 0;

            return new ConfigDropdown<T>(values, names, defaultIndex);
        }

        // A heinous bridge from reflection to generics
        private static readonly MethodInfo unboundFieldEnumDropdownMethod = typeof(ConfigBuilder).GetMethod(nameof(FieldEnumDropdown), BindingFlags.Static | BindingFlags.NonPublic);
        private static ConfigValueElement FieldEnumDropdownUntyped(object baseValue)
        {
            MethodInfo boundMethod = unboundFieldEnumDropdownMethod.MakeGenericMethod(baseValue.GetType());
            return (ConfigValueElement)boundMethod.Invoke(null, [baseValue]);
        }

        #endregion

        List<SerializedConfiggable> _data;

        private List<SerializedConfiggable> data
        {
            get
            {
                if(_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }

        internal void LoadData()
        {
            string folderPath = Path.Combine(Paths.DataFolder, owner.GetName().Name);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, GUID+".json");

            List<SerializedConfiggable> loadedData = new List<SerializedConfiggable>();


            if (!File.Exists(filePath))
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(loadedData);
                File.WriteAllText(filePath, json);
            }
            else
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    loadedData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SerializedConfiggable>>(json);
                    Debug.Log($"Loaded Config {GUID} with {loadedData.Count} values");
                }catch (System.Exception ex)
                {
                    Debug.LogError("Error Loading Configgy Data!");
                    Debug.LogException(ex);

                    int count = 0;
                    while(File.Exists(filePath+$".backup{((count > 0) ? $" ({count})": "")}"))
                    {
                        count++;
                    }

                    Debug.Log("Created configgy file backup.");
                    File.Copy(filePath, filePath + $".backup{((count > 0) ? $" ({count})" : "")}");
                    File.Delete(filePath);

                    loadedData = new List<SerializedConfiggable>();
                }
            }

            //Remove null values.
            loadedData = loadedData.Where(x => x.IsValid()).ToList();

            _data = loadedData;
        }

        public void SaveData()
        {
            if (_data == null)
                return;

            string folderPath = Path.Combine(Paths.DataFolder, owner.GetName().Name);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, GUID + ".json");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(_data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private bool saveNextFrame;

        internal void SaveDeferred()
        {
            saveNextFrame = true;
        }

        private IEnumerator SaveChecker()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (saveNextFrame)
                {
                    SaveData();
                    saveNextFrame = false;
                }
            }
        }


        internal void CreateSaverObject()
        {
            GameObject saveChecker = new GameObject($"Configgy_Saver ({GUID})");
            BehaviourRelay br = saveChecker.AddComponent<BehaviourRelay>();
            br.StartCoroutine(SaveChecker());
            GameObject.DontDestroyOnLoad(saveChecker);
        }

        //Load config data.

        internal T GetValueAtAddress<T>(string address)
        {
            foreach (var sc in data)
            {
                if (sc.key == address)
                    return sc.GetValue<T>();
            }

            return default(T);
        }

        internal bool TryGetValueAtAddress<T>(string address, out T value)
        {
            value = default(T);

            foreach (var sc in data)
            {
                if (sc.key == address)
                {
                    try
                    {
                        value = sc.GetValue<T>();
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Failed to deserialize value");
                        Debug.LogException(ex);
                        break;
                    }
                }
            }

            return false;
        }

        internal void SetValueAtAddress(string address, object value)
        {
            foreach (var sc in data)
            {
                if (sc.key == address)
                {
                    sc.SetValue(value);
                    return;
                }
            }

            data.Add(new SerializedConfiggable(address, value));
        }
    }
}
