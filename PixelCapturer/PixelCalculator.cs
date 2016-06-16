using PixelCapturer.LightsConfiguration;

namespace PixelCapturer
{
    public class PixelCalculator
    {
        private readonly LightConfiguration _config;

        public PixelCalculator(LightConfiguration config)
        {
            _config = config;
        }

        public Coordinate[,] Calculate(Display surface)
        {
            var pixelOffset = new Coordinate[_config.LedCount, 256];
            var x = new int[16];
            var y = new int[16];

            var displayIdx = 0;
            var ledIdx = 0;
            foreach (var display in _config.Displays)
            {
                foreach (var led in display.Leds)
                {
                    var range = surface.Width / (float)display.Columns;
                    var step = range / 16.0;
                    var start = range * led.Column + step * 0.5;
                    for (var col = 0; col < 16; col++)
                    {
                        x[col] = (int)(start + step * col);
                    }

                    range = surface.Height / (float)display.Rows;
                    step = range / 16.0;
                    start = range * led.Row + step * 0.5;
                    for (var row = 0; row < 16; row++)
                    {
                        y[row] = (int)(start + step * row);
                    }

                    for (var row = 0; row < 16; row++)
                    {
                        for (var col = 0; col < 16; col++)
                        {
                            pixelOffset[ledIdx, row * 16 + col] = new Coordinate
                            {
                                Y = y[row],
                                X = x[col],
                                Display = displayIdx
                            };
                        }
                    }
                    ledIdx++;
                }
                displayIdx++;
            }
            return pixelOffset;
        }
    }
}