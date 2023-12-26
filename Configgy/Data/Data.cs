using HydraDynamics.DataPersistence;
using System.Collections.Generic;
using System.Linq;

namespace Configgy
{
    public static class Data
    {
        public static DataFile<Config> Config = new DataFile<Config>(new Config(), "config.txt");
    }

    public class Config : Validatable
    {
        public List<SerializedConfiggable> configgables;

        public override bool AllowExternalRead => false;

        public Config()
        {
            configgables = new List<SerializedConfiggable>();
        }

        public override bool Validate()
        {
            return configgables != null;
        }

        public bool ContainsAddress(string key)
        {
            for(int i = 0; i < configgables.Count; i++)
            {
                if (configgables[i] == null)
                    continue;

                if (configgables[i].key != key)
                    continue;

                return true;
            }

            return false;
        }

        public SerializedConfiggable Get(string key)
        {
            for(int i = 0; i < configgables.Count; i++)
            {
                if (configgables[i].key == key)
                    return configgables[i];
            }

            return null;
        }
    }
}
