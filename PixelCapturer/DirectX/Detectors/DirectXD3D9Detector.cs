using System;
using PixelCapturer.DirectX.Handlers;
using PixelCapturer.DirectX.Interceptors;
using PixelCapturer.Logging;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D9Detector : IDirectXDetector
    {
        private readonly IDirect3DDevice9Handler _handler;
        private readonly ILogger _logger = LoggerFactory.Create<DirectXD3D9Detector>();

        public DirectXD3D9Detector(IDirect3DDevice9Handler handler)
        {
            _handler = handler;
        }

        public bool TryDetect(out IDirectXInterceptor directXInterceptor)
        {
            if (NativeMethods.GetModuleHandle("d3d9.dll") != IntPtr.Zero)
            {
                _logger.Log("Intercepting d3d9.dll");
                var interceptor = new Direct3DDevice9Interceptor(_logger);
                interceptor.OnEndScene(_handler.EndSceneDelegate);
                directXInterceptor = interceptor;
                return true;
            }
            _logger.Log("d3d9.dll not found");
            directXInterceptor = null;
            return false;
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}