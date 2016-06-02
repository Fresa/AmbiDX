using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;

namespace PixelCapturer
{
    public class FrontbufferCapturer : ICapturingProcess
    {
        private readonly ColorMapper _colorMapper;
        readonly Device _device;
        private readonly Surface _surface;
        private readonly AdapterInformation _adapter;
        private Task _capturingTask;
        private Display _display;
        private Coordinate[,] _pixelOffset;

        public FrontbufferCapturer(int screen)
        {
            var direct = new Direct3D();

            if (direct.Adapters.Any(information => information.Adapter == screen) == false)
            {
                throw new NotSupportedException($"No display adapter with number {screen}. Choose one of {string.Join<int>(", ", direct.Adapters.Select(information => information.Adapter))}");
            }
            _adapter = direct.Adapters.First(information => information.Adapter == screen);

            _display = new Display
            {
                Height = _adapter.CurrentDisplayMode.Height,
                Width = _adapter.CurrentDisplayMode.Width
            };

            var presentParams = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                EnableAutoDepthStencil = false,
                MultiSampleType = MultisampleType.None,
                BackBufferHeight = _display.Height,
                BackBufferWidth = _display.Width
            };

            _device = new Device(direct, _adapter.Adapter, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, presentParams);
            _surface = Surface.CreateOffscreenPlain(_device, _display.Width, _display.Height, Format.A8R8G8B8, Pool.SystemMemory);

            var prevColorHolder = new PrevColorHolder();
            var pixelCalculator = new PixelCalculator();
            _pixelOffset = pixelCalculator.Calculate(new[] { _display });
            _colorMapper = new ColorMapper(prevColorHolder, new GammaCorrection());
        }

        private Surface CaptureScreen()
        {
            //var s = _d.GetBackBuffer(0, 0);
            //var stre = Surface.ToStream(s, ImageFileFormat.Bmp);
            //var a = s.LockRectangle(LockFlags.None);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _device.GetFrontBufferData(0, _surface);
            Console.WriteLine($"Front buffer FPS: {1000 / watch.ElapsedMilliseconds}");
            //Surface.ToFile(_surface, "test.bmp", ImageFileFormat.Bmp);
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