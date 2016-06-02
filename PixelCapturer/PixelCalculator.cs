using System.Collections.Generic;

namespace PixelCapturer
{
    public class PixelCalculator
    {
        public Coordinate[,] Calculate(IReadOnlyList<Display> displays)
        {
            var leds = Config.Leds;
            var pixelOffset = new Coordinate[leds.Length, 256];
            var x = new int[16];
            var y = new int[16];
            // Precompute locations of every pixel to read when downsampling.
            // Saves a bunch of math on each frame, at the expense of a chunk
            // of RAM.  Number of samples is now fixed at 256; this allows for
            // some crazy optimizations in the downsampling code.
            for (var i = 0; i < leds.Length; i++) // For each LED...
            {
                var displayIdx = leds[i][0]; // Corresponding display index
                
                // Precompute columns, rows of each sampled point for this LED
                var range = displays[displayIdx].Width / (float)Config.Displays[displayIdx][1];
                var step = range / 16.0;
                var start = range * leds[i][1] + step * 0.5;
                for (var col = 0; col < 16; col++)
                {
                    x[col] = (int)(start + step * col);
                }

                range = displays[displayIdx].Height / (float)Config.Displays[displayIdx][2];
                step = range / 16.0;
                start = range * leds[i][2] + step * 0.5;
                for (var row = 0; row < 16; row++)
                {
                    y[row] = (int)(start + step * row);
                }

                // Get offset to each pixel within full screen capture
                for (var row = 0; row < 16; row++)
                {
                    for (var col = 0; col < 16; col++)
                    {
                        pixelOffset[i, row * 16 + col] = new Coordinate
                        {
                            Y = y[row],
                            X = x[col],
                            Display = displayIdx
                        };
                    }
                }

            }
            return pixelOffset;
        }
    }
}