using BepInEx.Configuration;

using UnityEngine;

namespace Configgy
{
    public static class Extensions
    {
        public static Transform[] GetChildren(this Transform tf)
        {
            int childCount = tf.childCount;
            Transform[] children = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                children[i] = tf.GetChild(i);
            }
            return children;
        }

        public static T GetDefault<T>(this ConfigEntry<T> entry) => (T)entry.DefaultValue;
    }
}
