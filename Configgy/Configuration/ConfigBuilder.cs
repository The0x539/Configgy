using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Configgy
{
    public class ConfigBuilder
    {
        public string GUID { get; }
        public string OwnerDisplayName { get; }
        public bool Initialized { get; private set; }

        private Assembly owner;

        private List<IConfigElement> _configElements;
        private IConfigElement[] configElements
        {
            get
            {
                if (_configElements == null)
                {
                    Build();
                }
                return _configElements.ToArray();
            }
        }

        public ConfigBuilder(string guid = null, string menuDisplayName = null) 
        {
            this.owner = Assembly.GetCallingAssembly();
            this.GUID = (string.IsNullOrEmpty(guid) ? owner.GetName().Name : guid);
            this.OwnerDisplayName = (string.IsNullOrEmpty(menuDisplayName) ? GUID : menuDisplayName);
        }

        public void Build()
        {
            if (Initialized)
                return;

            _configElements = new List<IConfigElement>();
            currentAssembly = owner;
            
            foreach (Type type in owner.GetTypes())
            {
                currentType = type;
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    ProcessMethod(method);
                }

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    ProcessField(field);
                }

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    //ProcessProperty(property);
                }
            }

            foreach (var element in _configElements)
            {
                element.BindConfig(this);
            }

            CreateSaverObject();

            OnConfigElementsChanged += (v) => ConfigurationManager.SubMenuElementsChanged();
            OnConfigElementsChanged?.Invoke(_configElements.ToArray());

            Initialized = true;
            ConfigurationManager.RegisterConfiguraitonMenu(this);
        }

        public event Action<IConfigElement[]> OnConfigElementsChanged;

        public IConfigElement[] GetConfigElements()
        {
            return configElements;
        }

        private Assembly currentAssembly;
        private Type currentType;

        private void ProcessMethod(MethodInfo method)
        {
            if (!method.IsStatic) //no instance!!
                return;

            if (method.ReturnType != typeof(void))
                return;

            if (method.GetParameters().Length > 0)
                return;

            ConfiggableAttribute cfg = method.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}"); //This isnt needed, but who cares.

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(method.Name);

            RegisterMethodAsButton(cfg, method);
        }

        private void ProcessField(FieldInfo field)
        {
            if (!field.IsStatic) //no instance!!
                return;

            ConfiggableAttribute cfg = field.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

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

        private void ProcessProperty(PropertyInfo property)
        {
            if (!property.CanWrite) //no instance!!
                return;

            if (!property.CanRead)
                return;

            ConfiggableAttribute cfg = property.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{property.DeclaringType.Namespace}.{property.DeclaringType.Name}.{property.Name}");

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(property.Name);

            if (typeof(IConfigElement).IsAssignableFrom(property.PropertyType))
            {
                IConfigElement cfgElement = (IConfigElement)property.GetValue(null);
                RegisterElementCore(cfg, cfgElement);
            }
            else
            {
                //RegisterPrimitive(cfg, field);
            }
        }

        private void RegisterMethodAsButton(ConfiggableAttribute descriptor, MethodInfo method)
        {
            ConfigButton button = new ConfigButton(() =>
            {
                method.Invoke(null, null);
            });

            button.BindDescriptor(descriptor);
            RegisterElementCore(descriptor, button);
        }

        private void RegisterElementCore(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            configElement.BindDescriptor(descriptor);
            _configElements.Add(configElement);
        }

        public void RegisterElement(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            if (!Initialized)
                Build();

            RegisterElementCore(descriptor, configElement);

            if(Initialized)
                OnConfigElementsChanged?.Invoke(_configElements.ToArray());
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
            }

            if (field.FieldType == typeof(Color))
            {
                Color baseValue = (Color)field.GetValue(null);
                ConfigColor colorElement = new ConfigColor(baseValue);
                colorElement.OnValueChanged += (v) => field.SetValue(null, v);
                colorElement.BindDescriptor(descriptor);
                RegisterElementCore(descriptor, colorElement);
            }
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
