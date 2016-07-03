using System;
using SharpDX.DXGI;

namespace PixelCapturer.DirectX.Handlers
{
    public interface IDirect3DDevice12Handler : IDisposable
    {
        void Present1Delegate(SwapChain1 swapChain);
        void PresentDelegate(SwapChain swapChain);
    }
}