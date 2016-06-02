using System;
using PixelCapturer.DirectX.Handlers;
using PixelCapturer.DirectX.Interceptors;
using PixelCapturer.Logging;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D11Detector : IDirectXDetector
    {
        private readonly IDirect3DDevice11Handler _handler;
        private readonly ILogger _logger = LoggerFactory.Create<DirectXD3D11Detector>();

        public DirectXD3D11Detector(IDirect3DDevice11Handler handler)
        {
            _handler = handler;
        }

        public bool TryDetect(out IDirectXInterceptor directXInterceptor)
        {
            if (NativeMethods.GetModuleHandle("d3d11.dll") != IntPtr.Zero)
            {
                _logger.Log("Intercepting d3d11.dll");
                var interceptor = new Direct3DDevice11Interceptor();
                interceptor.OnPresent(_handler.PresentDelegate);
                directXInterceptor = interceptor;
                return true;
            }
            _logger.Log("d3d11.dll not found");
            directXInterceptor = null;
            return false;
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}