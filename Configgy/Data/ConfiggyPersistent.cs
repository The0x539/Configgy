using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Configgy
{
    /// <summary>
    /// Serializes values without a UI element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConfiggyPersistent<T> : ConfigValueElement<T> where T : IConfigElement
    {
        public ConfiggyPersistent(T defaultValue) : base(defaultValue) {}
        protected override void BuildElementCore(ConfiggableAttribute descriptor, RectTransform rect) {}
        protected override void RefreshElementValueCore() {}
    }
}
