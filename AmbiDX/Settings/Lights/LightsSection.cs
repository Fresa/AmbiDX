using System.Configuration;
using AmbiDX.Settings.Process;

namespace AmbiDX.Settings.Lights
{
    public class LightsSection : ConfigurationSection
    {
        [ConfigurationProperty("minBrightness", IsRequired = true)]
        public byte MinBrightness => (byte)base["minBrightness"];

        [ConfigurationProperty("fade", IsRequired = true)]
        public byte Fade => (byte)base["fade"];

        [ConfigurationProperty("displays", IsRequired = true, IsDefaultCollection = false)]
        public DisplayCollection Displays => (DisplayCollection)base["displays"];
    }
}