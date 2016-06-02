using System.IO;
using SharpDX;

namespace PixelCapturer
{
    public class ColorMapper
    {
        private readonly PrevColorHolder _prevColorHolder;
        private readonly GammaCorrection _gammaCorrection;
        private readonly int _weight;

        public ColorMapper(PrevColorHolder prevColorHolder, GammaCorrection gammaCorrection)
        {
            _prevColorHolder = prevColorHolder;
            _gammaCorrection = gammaCorrection;
            _weight = 256 - Config.Fade;
        }

        public int[,] Map(DataStream stream, DataRectangle rectangle, Coordinate[,] pixelOffsets)
        {
            var ledColor = new int[pixelOffsets.GetLength(0), 3];
            var gammaCorrectedLedColor = new int[pixelOffsets.GetLength(0), 3];

            //var rectangles = surfaces.Select(surface => surface.LockRectangle(LockFlags.None)).ToArray();

            for (var i = 0; i < pixelOffsets.GetLength(0); i++)
            {
                var r = 0;
                var g = 0;
                var b = 0;

                for (var o = 0; o < 256; o++)
                {
                    var coordinate = pixelOffsets[i, o];
                    var buffer = new byte[4];
                    //var rectangle = rectangles[coordinate.Display];
                    
                    stream.Seek((coordinate.Y * rectangle.Pitch) + (coordinate.X * 4), SeekOrigin.Begin);
                    stream.Read(buffer, 0, 4);

                    //    var a = buffer[3];
                    r += buffer[2];
                    g += buffer[1];
                    b += buffer[0];
                }

                // Blend new pixel value with the value from the prior frame
                ledColor[i, 0] = (r / 256 * _weight + _prevColorHolder.PrevColor[i, 0] * Config.Fade) / 256;
                ledColor[i, 1] = (g / 256 * _weight + _prevColorHolder.PrevColor[i, 1] * Config.Fade) / 256;
                ledColor[i, 2] = (b / 256 * _weight + _prevColorHolder.PrevColor[i, 2] * Config.Fade) / 256;

                // Boost pixels that fall below the minimum brightness
                var sum = ledColor[i, 0] + ledColor[i, 1] + ledColor[i, 2];
                if (sum < Config.MinBrightness)
                {
                    if (sum == 0)
                    {
                        // To avoid divide-by-zero
                        const int deficit = Config.MinBrightness / 3; // Spread equally to R,G,B
                        ledColor[i, 0] += deficit;
                        ledColor[i, 1] += deficit;
                        ledColor[i, 2] += deficit;
                    }
                    else
                    {
                        var deficit = Config.MinBrightness - sum;
                        var s2 = sum * 2;
                        // Spread the "brightness deficit" back into R,G,B in proportion to
                        // their individual contribition to that deficit.  Rather than simply
                        // boosting all pixels at the low end, this allows deep (but saturated)
                        // colors to stay saturated...they don't "pink out."
                        ledColor[i, 0] += deficit * (sum - ledColor[i, 0]) / s2;
                        ledColor[i, 1] += deficit * (sum - ledColor[i, 1]) / s2;
                        ledColor[i, 2] += deficit * (sum - ledColor[i, 2]) / s2;
                    }
                }

                gammaCorrectedLedColor[i, 0] = _gammaCorrection.GammaTable[ledColor[i, 0], 0];
                gammaCorrectedLedColor[i, 1] = _gammaCorrection.GammaTable[ledColor[i, 1], 1];
                gammaCorrectedLedColor[i, 2] = _gammaCorrection.GammaTable[ledColor[i, 2], 2];
            }

            _prevColorHolder.Set(ledColor);

            //foreach (var surface in surfaces)
            //{
            //    surface.UnlockRectangle();
            //}
            
            return gammaCorrectedLedColor;
        }
    }
}