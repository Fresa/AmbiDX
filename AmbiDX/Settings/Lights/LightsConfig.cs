using System.Configuration;
using System.Linq;
using PixelCapturer.LightsConfiguration;

namespace AmbiDX.Settings.Lights
{
    public static class LightsConfig
    {
        private const string ConfigSection = "lights";

        public static LightsSection Get()
        {
            return Section;
        }

        public static int LedCount
        {
            get { return Get().Displays.Aggregate(0, (count, display) => count + display.Leds.Count); }
        }

        public static LightConfiguration ToLightConfiguration()
        {
            return new LightConfiguration(
                Section.Displays.Select(display => 
                    new DisplayConfig(
                        display.Leds.Select(led => 
                            new LedConfig(led.Column, led.Row)), 
                        display.Columns, 
                        display.Rows)), 
                Section.MinBrightness, 
                Section.Fade);
        }

        private static LightsSection Section
        {
            get
            {
                var section = ConfigurationManager.GetSection(ConfigSection) as LightsSection;
                if (section == null)
                {
                    throw new ConfigurationErrorsException(
                        $"Could not find {ConfigSection} configuration in app.config");
                }
                return section;
            }
        }
    }
}