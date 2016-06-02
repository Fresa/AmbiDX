using System;

namespace PixelCapturer
{
    public class GammaCorrection
    {
        public byte[,] GammaTable { get; }

        public GammaCorrection()
        {
            GammaTable = new byte[256, 3];
            for (var i = 0; i < 256; i++)
            {
                var f = Math.Pow(i / 255.0, 2.8);
                GammaTable[i, 0] = (byte)(f * 255.0);
                GammaTable[i, 1] = (byte)(f * 240.0);
                GammaTable[i, 2] = (byte)(f * 220.0);
            }
        }
    }
}