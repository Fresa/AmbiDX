using System.Configuration;

namespace AmbiDX.Settings.Preview
{
    public class PreviewSection : ConfigurationSection
    {
        [ConfigurationProperty("pixelSize", IsRequired = true)]
        public byte PixelSize => (byte)base["pixelSize"];
    }
}