using System.Diagnostics;
using System.Threading;
using PixelCapturer.Logging;
using SharpDX;
using SharpDX.Direct3D9;

namespace PixelCapturer.DirectX.Handlers
{
    public class D3D9PixelHandler : IDirect3DDevice9Handler
    {
        private readonly CaptureClient _client;
        private readonly ColorMapper _colorMapper;
        private readonly PixelCalculator _pixelCalculator;
        private readonly ILogger _logger = LoggerFactory.Create<D3D9PixelHandler>();
        private Display _display;
        private Coordinate[,] _pixelOffset;
        private Surface _offScreenSurface;
        private Surface _resolvedRenderTarget;
        private readonly object _disposedLock = new object();
        private readonly AutoResetEvent _dataAvailable = new AutoResetEvent(false);
        private CancellationTokenSource _cancel;
        private int[,] _pixelData;

        public D3D9PixelHandler(CaptureClient client, ColorMapper colorMapper, PixelCalculator pixelCalculator)
        {
            _client = client;
            _colorMapper = colorMapper;
            _pixelCalculator = pixelCalculator;
            StartPostProcessesingTask();
        }

        public void EndSceneDelegate(Device device)
        {
            var watch = Stopwatch.StartNew();

            using (var renderTarget = device.GetRenderTarget(0))
            {
                if (Monitor.TryEnter(_disposedLock))
                {
                    try
                    {
                        SetupResources(device, renderTarget);

                        device.StretchRectangle(renderTarget, _resolvedRenderTarget, TextureFilter.None);
                        device.GetRenderTargetData(_resolvedRenderTarget, _offScreenSurface);

                        DataStream dataStream;
                        var dataRectangle = _offScreenSurface.LockRectangle(LockFlags.ReadOnly, out dataStream);
                        _pixelData = _colorMapper.Map(dataStream, dataRectangle, _pixelOffset);
                        _offScreenSurface.UnlockRectangle();

                        _dataAvailable.Set();
                    }
                    finally
                    {
                        Monitor.Exit(_disposedLock);
                    }
                }
            }
            //_logger.Log("Time: {0}", watch.ElapsedMilliseconds);
            watch.Reset();
        }

        private void SetupResources(Device device, Surface renderTarget)
        {
            var display = new Display
            {
                Height = renderTarget.Description.Height,
                Width = renderTarget.Description.Width
            };

            if (_display == null || display.Height != _display.Height || display.Width != _display.Width)
            {
                _display = display;
                _pixelOffset = _pixelCalculator.Calculate(_display);
                _offScreenSurface = Surface.CreateOffscreenPlain(device, display.Width, display.Height, renderTarget.Description.Format, Pool.SystemMemory);
                _resolvedRenderTarget = Surface.CreateRenderTarget(device, display.Width, display.Height, renderTarget.Description.Format, MultisampleType.None, 0, false);
            }
        }

        private void StartPostProcessesingTask()
        {
            _cancel = new CancellationTokenSource();
            var calculationTask = new Thread(() =>
            {
                while (true)
                {
                    _dataAvailable.WaitOne();
                    if (_cancel.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    _client.StreamData(_pixelData);
                }
            });
            calculationTask.Start();
        }

        public void Dispose()
        {
            lock (_disposedLock)
            {
                _logger.Log("Disposing");
                _offScreenSurface?.Dispose();
                _resolvedRenderTarget?.Dispose();
                _cancel.Cancel();
                _dataAvailable.Set();
            }
        }
    }
}