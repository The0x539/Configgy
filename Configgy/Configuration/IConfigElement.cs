using UnityEngine;

namespace Configgy
{
    public interface IConfigElement
    {
        public void BindDescriptor (ConfiggableAttribute configgable);
        public ConfiggableAttribute GetDescriptor();
        public void BuildElement(RectTransform rect);

        public void OnMenuOpen();
        public void OnMenuClose();
        public void BindConfig(ConfigBuilder configBuilder);
    }
}
