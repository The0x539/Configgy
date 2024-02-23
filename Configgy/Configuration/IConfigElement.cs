using UnityEngine;

namespace Configgy
{
    public interface IConfigElement
    {
        internal void BindDescriptor (ConfiggableAttribute configgable);
        internal ConfiggableAttribute GetDescriptor();
        public void BuildElement(RectTransform rect);

        public void OnMenuOpen();
        public void OnMenuClose();
        internal void BindConfig(ConfigBuilder configBuilder);
    }
}
