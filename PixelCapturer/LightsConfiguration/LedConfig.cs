using System;

namespace PixelCapturer.LightsConfiguration
{
    [Serializable]
    public class LedConfig
    {
        public LedConfig(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; private set; }
        public int Row { get; private set; }
    }
}