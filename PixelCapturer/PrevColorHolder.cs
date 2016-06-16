using System;
using PixelCapturer.LightsConfiguration;

namespace PixelCapturer
{
    public class PrevColorHolder
    {
        public int[,] PrevColor { get; }

        public PrevColorHolder(LightConfiguration config)
        {
            PrevColor = new int[config.LedCount, 3];
            for (var i = 0; i < PrevColor.GetLength(0); i++)
            {
                PrevColor[i, 0] = PrevColor[i, 1] = PrevColor[i, 2] = config.MinBrightness / 3;
            }
        }

        public void Set(int[,] ledColor)
        {
            Array.Copy(ledColor, PrevColor, ledColor.Length);
        }
    }
}