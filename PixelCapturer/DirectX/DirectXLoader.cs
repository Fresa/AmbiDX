using System;
using System.Collections.Generic;
using System.Linq;
using PixelCapturer.DirectX.Detectors;
using PixelCapturer.DirectX.Interceptors;
using PixelCapturer.Logging;

namespace PixelCapturer.DirectX
{
    public class DirectXLoader : IDisposable
    {
        private readonly ILogger _logger = LoggerFactory.Create<DirectXLoader>();
        private readonly IEnumerable<IDirectXDetector> _directXInterceptors;

        public DirectXLoader(params IDirectXDetector[] directXInterceptors)
        {
            _directXInterceptors = directXInterceptors;
        }

        public IEnumerable<IDirectXInterceptor> Load()
        {
            _logger.Log("Got {0} interceptors", _directXInterceptors.Count());

            var interceptors = new List<IDirectXInterceptor>();
            foreach (var directXInterceptor in _directXInterceptors)
            {
                IDirectXInterceptor interceptor;
                _logger.Log("Detecting using {0}", directXInterceptor.GetType().FullName);
                if (directXInterceptor.TryDetect(out interceptor))
                {
                    interceptors.Add(interceptor);
                }
            }
            return interceptors;
        }

        public void Dispose()
        {
            foreach (var directXInterceptor in _directXInterceptors)
            {
                directXInterceptor.Dispose();
            }
        }
    }
}