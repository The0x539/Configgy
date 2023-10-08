using UnityEngine;

namespace Configgy
{
    public interface IConfigElement
    {
        public void BindDescriptor (Configgable configgable);
        public Configgable GetDescriptor();
        public void BuildElement(RectTransform rect);

        public void OnMenuOpen();
        public void OnMenuClose();
    }
}
