using System;

namespace PixelCapturer
{
    public class PrevColorHolder
    {
        public int[,] PrevColor { get; }

        public PrevColorHolder()
        {
            PrevColor = new int[Config.Leds.Length, 3];
            for (var i = 0; i < PrevColor.GetLength(0); i++)
            {
                PrevColor[i, 0] = PrevColor[i, 1] = PrevColor[i, 2] = Config.MinBrightness / 3;
            }
        }

        public void Set(int[,] ledColor)
        {
            Array.Copy(ledColor, PrevColor, ledColor.Length);
        }
    }
}