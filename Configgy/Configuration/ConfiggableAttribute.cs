using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Configgy
{
    /// <summary>
    /// Place this attribute on a primitive or Configgy types field to make them visible to the ConfigBuilder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class ConfiggableAttribute : Attribute
    {
        /// <summary>
        /// The path to the submenu that the attribute's field will be drawn to in the config menu.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Label of the config element in the config menu.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Description of the setting's purpose in the config menu.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The order in which the element is displayed on its respective page.
        /// </summary>
        public int OrderInList { get; }


        /// <summary>
        /// The address at which the field will be serialized to by Configgy.
        /// </summary>
        public string SerializationAddress { get; private set; }

        /// <summary>
        /// The config builder that owns this attribute.
        /// </summary>
        public ConfigBuilder Owner { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="path">Optional: Which submenu your element will be drawn to in the config menu. ex: "Group1/Group2" defaults to the root of your mod's submenu</param>
        /// <param name="displayName">Optional: The label on the element, defaults to the field's name if empty</param>
        /// <param name="orderInList">Optional: The order in which the element is displayed on its respective page, defaults to 0</param>
        /// <param name="description">Optional: A description of what your setting is. Defaults to no description</param>
        public ConfiggableAttribute(string path = "", string displayName = null, int orderInList = 0, string description = null) 
        {
            this.Path = path;
            this.DisplayName = displayName;
            this.Description = description;
            this.OrderInList = orderInList;
        }

        //Todo figure out a better solution to this please.
        public void SetSerializationAddress(string address)
        {
            this.SerializationAddress = address;
        }

        public void SetDisplayName(string name)
        {
            this.DisplayName = name;
        }

        public void SetDescription(string description)
        {
            this.Description = description;
        }

        
        public void SetOwner(ConfigBuilder owner)
        {
            if (Owner != null)
                return;

            Owner = owner;
        }

        public void SetDisplayNameFromCamelCase(string camelCaseName)
        {
            string newName = camelCaseName;
            newName = Regex.Replace(newName, "^_", "").Trim();
            newName = Regex.Replace(newName, "([a-z])([A-Z])", "$1 $2").Trim();
            newName = Regex.Replace(newName, "([A-Z])([A-Z][a-z])", "$1 $2").Trim();
            newName = string.Concat(newName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

            if (newName.Length > 0)
                if (char.IsLower(newName[0]))
                {
                    char startChar = newName[0];
                    newName = newName.Remove(0, 1);
                    newName = char.ToUpper(startChar) + newName;
                }

            this.DisplayName = newName;
        }
    }
}
