using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixelCapturer.LightsConfiguration
{
    [Serializable]
    public class DisplayConfig
    {
        public DisplayConfig(IEnumerable<LedConfig> leds, int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            Leds = new ReadOnlyCollection<LedConfig>(leds.ToList());
        }

        public int Columns { get; private set; }

        public int Rows { get; private set; }

        public IReadOnlyList<LedConfig> Leds { get; private set; }
    }
}