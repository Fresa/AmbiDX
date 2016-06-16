using System.Configuration;

namespace AmbiDX.Settings.Lights
{
    public class LedElement : ConfigurationElement
    {
        [ConfigurationProperty("column", IsRequired = true)]
        public int Column => (int)base["column"];

        [ConfigurationProperty("row", IsRequired = true)]
        public int Row => (int)base["row"];
    }
}