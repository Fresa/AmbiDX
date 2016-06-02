
using System;
using SharpDX.DXGI;

namespace PixelCapturer.DirectX.Handlers
{
    public interface IDirect3DDevice11Handler : IDisposable
    {
        void PresentDelegate(SwapChain swapChain);
    }
}