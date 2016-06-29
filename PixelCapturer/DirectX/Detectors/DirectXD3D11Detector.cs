using PixelCapturer.DirectX.Handlers;
using PixelCapturer.DirectX.Interceptors;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D11Detector : DirectXDetector
    {
        private readonly IDirect3DDevice11Handler _handler;
        private const string DirectXDllFileName = "d3d11.dll";

        public DirectXD3D11Detector(IDirect3DDevice11Handler handler) : base(DirectXDllFileName)
        {
            _handler = handler;
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            var interceptor = new Direct3DDevice11Interceptor();
            interceptor.OnPresent(_handler.PresentDelegate);
            return interceptor;
        }

        public override void Dispose()
        {
            _handler.Dispose();
        }
    }
}