using System.Configuration;

namespace AmbiDX.Settings.Lights
{
    public class DisplayElement : ConfigurationElement
    {
        [ConfigurationProperty("columns", IsRequired = true)]
        public int Columns => (int)base["columns"];

        [ConfigurationProperty("rows", IsRequired = true)]
        public int Rows => (int)base["rows"];

        [ConfigurationProperty("leds", IsDefaultCollection = false)]
        public LedCollection Leds => (LedCollection)base["leds"];
    }
}