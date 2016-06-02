using PixelCapturer.Logging;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace PixelCapturer.DirectX.Handlers
{
    public class D3D11PixelHandler : IDirect3DDevice11Handler
    {
        private readonly CaptureClient _client;
        private readonly ColorMapper _colorMapper;
        private readonly PixelCalculator _pixelCalculator;
        private readonly ILogger _logger = LoggerFactory.Create<D3D11PixelHandler>();
        private Display _display;
        private Coordinate[,] _pixelOffset;
        private Device _device;
        private Texture2D _screenTexture;

        public D3D11PixelHandler(CaptureClient client, ColorMapper colorMapper, PixelCalculator pixelCalculator)
        {
            _client = client;
            _colorMapper = colorMapper;
            _pixelCalculator = pixelCalculator;           
        }

        public void PresentDelegate(SwapChain swapChain)
        {
            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
            {
                var display = new Display
                {
                    Height = backBuffer.Description.Height,
                    Width = backBuffer.Description.Width
                };

                CreateResources(display);

                _device.ImmediateContext.CopyResource(backBuffer, _screenTexture);
            }

            DataStream dataStream;

            var mapSource = _device.ImmediateContext.MapSubresource(_screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out dataStream);
            var dataRectangle = new DataRectangle
            {
                DataPointer = mapSource.DataPointer,
                Pitch = mapSource.RowPitch
            };

            var pixelData = _colorMapper.Map(dataStream, dataRectangle, _pixelOffset);
            _device.ImmediateContext.UnmapSubresource(_screenTexture, 0);

            _client.StreamData(pixelData);
        }

        private void CreateResources(Display display)
        {
            if (_device == null)
            {
                using (var renderForm = new RenderForm())
                {
                    SwapChain swapChain;
                    Device.CreateWithSwapChain(
                        DriverType.Hardware,
                        DeviceCreationFlags.BgraSupport,
                        new SwapChainDescription
                        {
                            BufferCount = 1,
                            Flags = SwapChainFlags.None,
                            IsWindowed = true,
                            ModeDescription =
                                new ModeDescription(100, 100, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                            OutputHandle = renderForm.Handle,
                            SampleDescription = new SampleDescription(1, 0),
                            SwapEffect = SwapEffect.Discard,
                            Usage = Usage.RenderTargetOutput
                        },
                        out _device,
                        out swapChain);
                }
            }

            if (_display == null || display.Height != _display.Height || display.Width != _display.Width)
            {
                _display = display;
                _pixelOffset = _pixelCalculator.Calculate(new[] { _display });

                var textureDesc = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = display.Width,
                    Height = display.Height,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging
                };
                _screenTexture = new Texture2D(_device, textureDesc);
            }
        }

        public void Dispose()
        {
            // Todo: handle this more graceful (multi-threading)
            _display = null;
            _device?.Dispose();
            _screenTexture?.Dispose();
        }
    }
}