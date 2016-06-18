using PixelCapturer.DirectX.Interceptors;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D10Detector : DirectXDetector
    {
        private const string DirectXDllFileName = "d3d10.dll";

        public DirectXD3D10Detector() : base(DirectXDllFileName)
        {
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            return new Direct3DDevice10Interceptor();
        }

        public override void Dispose()
        {
            
        }
    }
}