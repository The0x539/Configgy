using HydraDynamics.DataPersistence;
using System.Collections.Generic;

namespace Configgy
{
    public static class Data
    {
        public static DataFile<Config> Config = new DataFile<Config>(new Config(), "config.txt");
    }

    public class Config : Validatable
    {
        public Dictionary<string, object> Configgables;
        public override bool AllowExternalRead => false;

        public Config()
        {
            Configgables = new Dictionary<string, object>();
        }

        public override bool Validate()
        {
            return Configgables != null;
        }
    }
}
