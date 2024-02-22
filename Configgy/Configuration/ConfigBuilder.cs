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
        /// Forcefully rebuilds the configuration menu. Do not use this in a hot path.
        /// </summary>
        public void Rebuild()
        {
            BuildInternal();
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
        /// Manually registers a config element with a descriptor. Call <see cref="ConfigBuilder.Rebuild"/> to rebuild the menu after adding elements.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="configElement"></param>
        public void RegisterElement(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            BuildInternal();
            RegisterElementCore(descriptor, configElement);
        }


        private void RegisterPrimitive(ConfiggableAttribute descriptor, FieldInfo field)
        {
            if (field.FieldType == typeof(float))
            {
                RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

                ConfigValueElement<float> floatElement = null;

                if (range == null)
                {
                    float baseValue = (float)field.GetValue(null);
                    floatElement = new ConfigInputField<float>(baseValue);

                }
                else
                {
                    float baseValue = (float)field.GetValue(null);
                    float clampedValue = Mathf.Clamp(baseValue, range.min, range.max);
                    floatElement = new FloatSlider(clampedValue, range.min, range.max);
                }

                floatElement.OnValueChanged += (v) => field.SetValue(null, v); //this is cursed as hell lol, dont care
                floatElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, floatElement);
                return;
            }

            if (field.FieldType == typeof(int))
            {
                RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

                ConfigValueElement<int> intElement = null;

                int baseValue = (int)field.GetValue(null);

                if (range == null)
                {
                    intElement = new ConfigInputField<int>(baseValue);
                }
                else
                {
                    int clampedValue = Mathf.Clamp(baseValue, (int)range.min, (int)range.max);
                    intElement = new IntegerSlider(clampedValue, (int)range.min, (int)range.max);
                }

                intElement.OnValueChanged += (v) => field.SetValue(null, v);
                intElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, intElement);
                return;
            }

            if (field.FieldType == typeof(uint))
            {
                ConfigInputField<uint> uintElement = null;
                uint baseValue = (uint)field.GetValue(null);
                uintElement = new ConfigInputField<uint>(baseValue);

                uintElement.OnValueChanged += (v) => field.SetValue(null, v);
                uintElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, uintElement);
                return;
            }

            if (field.FieldType == typeof(long))
            {
                ConfigInputField<long> longElement = null;
                long baseValue = (long)field.GetValue(null);
                longElement = new ConfigInputField<long>(baseValue);

                longElement.OnValueChanged += (v) => field.SetValue(null, v);
                longElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, longElement);
                return;
            }

            if (field.FieldType == typeof(Quaternion))
            {
                Quaternion val = (Quaternion)field.GetValue(null);

                ConfigQuaternion QuaternionElement = new ConfigQuaternion(val);
                QuaternionElement.OnValueChanged += (v) => field.SetValue(null, v);
                QuaternionElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, QuaternionElement);
                return;
            }

            if (field.FieldType == typeof(Vector3))
            {
                Vector3 baseValue = (Vector3)field.GetValue(null);
                ConfigVector3 Vector3Element = new ConfigVector3(baseValue);
                Vector3Element.OnValueChanged += (v) => field.SetValue(null, v);
                Vector3Element.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, Vector3Element);
                return;
            }

            if (field.FieldType == typeof(Vector2))
            {
                Vector2 baseValue = (Vector2)field.GetValue(null);
                ConfigVector2 Vector2Element = new ConfigVector2(baseValue);
                Vector2Element.OnValueChanged += (v) => field.SetValue(null, v);
                Vector2Element.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, Vector2Element);
                return;
            }

            if (field.FieldType == typeof(ulong))
            {
                ConfigInputField<ulong> ulongElement = null;
                ulong baseValue = (ulong)field.GetValue(null);
                ulongElement = new ConfigInputField<ulong>(baseValue);

                ulongElement.OnValueChanged += (v) => field.SetValue(null, v);
                ulongElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, ulongElement);
                return;
            }

            if (field.FieldType == typeof(bool))
            {
                bool baseValue = (bool)field.GetValue(null);
                ConfigToggle toggleElement = new ConfigToggle(baseValue);
                toggleElement.OnValueChanged += (v) => field.SetValue(null, v);
                toggleElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, toggleElement);
            }

            if (field.FieldType == typeof(string))
            {
                string baseValue = (string)field.GetValue(null);
                ConfigInputField<string> stringElement = new ConfigInputField<string>(baseValue);
                stringElement.OnValueChanged += (v) => field.SetValue(null, v);
                stringElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, stringElement);
                return;
            }

            if (field.FieldType == typeof(char))
            {
                char baseValue = (char)field.GetValue(null);
                ConfigInputField<char> stringElement = new ConfigInputField<char>(baseValue);
                stringElement.OnValueChanged += (v) => field.SetValue(null, v);
                stringElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, stringElement);
                return;
            }

            if (field.FieldType == typeof(Color))
            {
                Color baseValue = (Color)field.GetValue(null);
                ConfigColor colorElement = new ConfigColor(baseValue);
                colorElement.OnValueChanged += (v) => field.SetValue(null, v);
                colorElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, colorElement);
                return;
            }

            if (field.FieldType.IsEnum)
            {
                ConfigDropdown<int> enumElement = null;
                int baseValue = (int)field.GetValue(null);

                List<int> values = new List<int>();
                foreach (var value in Enum.GetValues(field.FieldType))
                {
                    values.Add(Convert.ToInt32(value));
                }

                enumElement = new ConfigDropdown<int>(values.ToArray(), Enum.GetNames(field.FieldType), baseValue);
                enumElement.OnValueChanged += (v) => field.SetValue(null, v);
                enumElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, enumElement);
                return;
            }

            Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register field {field.DeclaringType.Name}.{field.Name}. But it's type ({field.FieldType.Name}) is not supported. Skipping!");
        }

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
