using System.IO;
using PixelCapturer.LightsConfiguration;
using SharpDX;

namespace PixelCapturer
{
    public class ColorMapper
    {
        private readonly PrevColorHolder _prevColorHolder;
        private readonly GammaCorrection _gammaCorrection;
        private readonly LightConfiguration _lightConfiguration;
        private readonly int _weight;

        public ColorMapper(PrevColorHolder prevColorHolder, GammaCorrection gammaCorrection, LightConfiguration lightConfiguration)
        {
            _prevColorHolder = prevColorHolder;
            _gammaCorrection = gammaCorrection;
            _lightConfiguration = lightConfiguration;
            _weight = 256 - _lightConfiguration.Fade;
        }

        public int[,] Map(DataStream stream, DataRectangle rectangle, Coordinate[,] pixelOffsets)
        {
            var ledColor = new int[pixelOffsets.GetLength(0), 3];
            var gammaCorrectedLedColor = new int[pixelOffsets.GetLength(0), 3];

            for (var i = 0; i < pixelOffsets.GetLength(0); i++)
            {
                var r = 0;
                var g = 0;
                var b = 0;

                for (var o = 0; o < 256; o++)
                {
                    var coordinate = pixelOffsets[i, o];
                    var buffer = new byte[4];
                    
                    stream.Seek((coordinate.Y * rectangle.Pitch) + (coordinate.X * 4), SeekOrigin.Begin);
                    stream.Read(buffer, 0, 4);

                    // var a = buffer[3];
                    r += buffer[2];
                    g += buffer[1];
                    b += buffer[0];
                }

                ledColor[i, 0] = (r / 256 * _weight + _prevColorHolder.PrevColor[i, 0] * _lightConfiguration.Fade) / 256;
                ledColor[i, 1] = (g / 256 * _weight + _prevColorHolder.PrevColor[i, 1] * _lightConfiguration.Fade) / 256;
                ledColor[i, 2] = (b / 256 * _weight + _prevColorHolder.PrevColor[i, 2] * _lightConfiguration.Fade) / 256;

                var sum = ledColor[i, 0] + ledColor[i, 1] + ledColor[i, 2];
                if (sum < _lightConfiguration.MinBrightness)
                {
                    if (sum == 0)
                    {
                        var deficit = _lightConfiguration.MinBrightness / 3;
                        ledColor[i, 0] += deficit;
                        ledColor[i, 1] += deficit;
                        ledColor[i, 2] += deficit;
                    }
                    else
                    {
                        var deficit = _lightConfiguration.MinBrightness - sum;
                        var s2 = sum * 2;
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
            
            return gammaCorrectedLedColor;
        }
    }
}