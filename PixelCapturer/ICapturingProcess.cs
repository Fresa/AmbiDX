using System;
using System.Threading;

namespace PixelCapturer
{
    public interface ICapturingProcess
    {
        void Start(int processId);
        void Stop();
        void OnScreenCaptured(Action<int[,]> ev, CancellationTokenSource cancellationToken);
    }
}