using System;
using PixelCapturer.DirectX.Interceptors;
using PixelCapturer.Logging;

namespace PixelCapturer.DirectX.Detectors
{
    public abstract class DirectXDetector : IDirectXDetector
    {
        private readonly string _directXDllFileName;
        private readonly ILogger _logger = LoggerFactory.Create<DirectXDetector>();

        protected DirectXDetector(string directXDllFileName)
        {
            _directXDllFileName = directXDllFileName;
        }

        protected abstract IDirectXInterceptor DirectXInterceptorFactory();

        public bool TryDetect(out IDirectXInterceptor directXInterceptor)
        {
            if (NativeMethods.GetModuleHandle(_directXDllFileName) != IntPtr.Zero)
            {
                _logger.Log($"Intercepting {_directXDllFileName}");
                directXInterceptor = DirectXInterceptorFactory();
                return true;
            }
            _logger.Log($"{_directXDllFileName} not found");
            directXInterceptor = null;
            return false;
        }

        public abstract void Dispose();
    }
}