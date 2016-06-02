using System;
using SharpDX.Direct3D9;

namespace PixelCapturer.DirectX.Handlers
{
    public interface IDirect3DDevice9Handler : IDisposable
    {
        void EndSceneDelegate(Device device);
    }
}