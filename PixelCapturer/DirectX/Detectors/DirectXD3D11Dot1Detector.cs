using PixelCapturer.DirectX.Interceptors;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D11Dot1Detector : DirectXDetector
    {
        private const string DirectXDllFileName = "d3d11_1.dll";

        public DirectXD3D11Dot1Detector() : base(DirectXDllFileName)
        {
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            return new Direct3DDevice11Interceptor();
        }

        public override void Dispose()
        {

        }
    }
}