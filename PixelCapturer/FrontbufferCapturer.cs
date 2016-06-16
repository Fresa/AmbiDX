using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PixelCapturer.LightsConfiguration;
using PixelCapturer.Logging;
using SharpDX;
using SharpDX.Direct3D9;

namespace PixelCapturer
{
    public class FrontbufferCapturer : ICapturingProcess
    {
        private readonly ColorMapper _colorMapper;
        readonly Device _device;
        private readonly Surface _surface;
        private Task _capturingTask;
        private readonly Coordinate[,] _pixelOffset;

        private readonly ILogger _logger = LoggerFactory.Create<FrontbufferCapturer>();

        public FrontbufferCapturer(int screen, LightConfiguration lightConfiguration)
        {
            var direct = new Direct3D();

            if (direct.Adapters.Any(information => information.Adapter == screen) == false)
            {
                throw new NotSupportedException($"No display adapter with number {screen}. Choose one of {string.Join<int>(", ", direct.Adapters.Select(information => information.Adapter))}");
            }
            var adapter = direct.Adapters.First(information => information.Adapter == screen);

            var display = new Display
            {
                Height = adapter.CurrentDisplayMode.Height,
                Width = adapter.CurrentDisplayMode.Width
            };

            var presentParams = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                EnableAutoDepthStencil = false,
                MultiSampleType = MultisampleType.None,
                BackBufferHeight = display.Height,
                BackBufferWidth = display.Width
            };

            _device = new Device(direct, adapter.Adapter, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, presentParams);
            _surface = Surface.CreateOffscreenPlain(_device, display.Width, display.Height, Format.A8R8G8B8, Pool.SystemMemory);

            var prevColorHolder = new PrevColorHolder(lightConfiguration);
            var pixelCalculator = new PixelCalculator(lightConfiguration);
            _pixelOffset = pixelCalculator.Calculate(display);
            _colorMapper = new ColorMapper(prevColorHolder, new GammaCorrection(), lightConfiguration);
        }

        private Surface CaptureScreen()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _device.GetFrontBufferData(0, _surface);
            _logger.Log($"Front buffer FPS: {1000 / watch.ElapsedMilliseconds}");
            return _surface;
        }

        public void Start(int processId)
        {
            _capturingTask.Start();
        }

        public void Stop()
        {
            
        }

        public void OnScreenCaptured(Action<int[,]> ev, CancellationTokenSource cancellationToken)
        {
            _capturingTask = new Task(() =>
            {
                while (true)
                {
                    if (cancellationToken.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    var surface = CaptureScreen();
                    DataStream dataStream;
                    var rectangle = surface.LockRectangle(LockFlags.None, out dataStream);
                    var data = _colorMapper.Map(dataStream, rectangle, _pixelOffset);
                    ev(data);
                    surface.UnlockRectangle();
                }
            }, TaskCreationOptions.LongRunning);
        }
    }

}