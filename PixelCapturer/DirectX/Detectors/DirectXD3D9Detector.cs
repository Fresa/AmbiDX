using PixelCapturer.DirectX.Handlers;
using PixelCapturer.DirectX.Interceptors;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D9Detector : DirectXDetector
    {
        private readonly IDirect3DDevice9Handler _handler;
        private const string DirectXDllFileName = "d3d9.dll";

        public DirectXD3D9Detector(IDirect3DDevice9Handler handler) : base(DirectXDllFileName)
        {
            _handler = handler;
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            var interceptor = new Direct3DDevice9Interceptor();
            interceptor.OnEndScene(_handler.EndSceneDelegate);
            return interceptor;
        }
        
        public override void Dispose()
        {
            _handler.Dispose();
        }
    }
}