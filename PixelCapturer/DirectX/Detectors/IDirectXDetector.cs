using System;
using PixelCapturer.DirectX.Interceptors;

namespace PixelCapturer.DirectX.Detectors
{
    public interface IDirectXDetector : IDisposable
    {
        bool TryDetect(out IDirectXInterceptor directXInterceptor);
    }
}