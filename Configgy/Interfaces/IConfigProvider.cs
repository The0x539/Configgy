using System;
using System.Collections.Generic;
using System.Text;

namespace Configgy.Interfaces
{
    public interface IConfigProvider
    {
        public IConfigSetting[] GetSettings();
        public IConfigSetting GetSetting(string key);
    }

    public interface IConfigSetting
    {
        public string Key { get; }
        public object GetValue();
        public void SetValue(object value);
    }

}
