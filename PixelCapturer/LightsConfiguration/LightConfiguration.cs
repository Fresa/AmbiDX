using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixelCapturer.LightsConfiguration
{
    [Serializable]
    public class LightConfiguration
    {
        public LightConfiguration(IEnumerable<DisplayConfig> displayConfigs, byte minBrightness, byte fade)
        {
            MinBrightness = minBrightness;
            Fade = fade;
            Displays = new ReadOnlyCollection<DisplayConfig>(displayConfigs.ToList());
        }

        public int LedCount
        {
            get { return Displays.Aggregate(0, (count, config) => count + config.Leds.Count); }
        }

        public IReadOnlyList<DisplayConfig> Displays { get; private set; }
        public byte MinBrightness { get; private set; }

        public byte Fade { get; private set; }
    }
}