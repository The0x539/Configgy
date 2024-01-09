using BepInEx.Configuration;
using UnityEngine;

namespace Configgy
{
    public interface IConfigElement
    {
        ConfigBuilder Parent { get; set; }
        ConfigDefinition Definition { get; }
        ConfigElementMetadata Metadata { get; }
        void BuildElement(RectTransform rect);
    }
}
