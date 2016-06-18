using PixelCapturer.DirectX.Interceptors;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D12Detector : DirectXDetector
    {
        private const string DirectXDllFileName = "d3d12.dll";

        public DirectXD3D12Detector() : base(DirectXDllFileName)
        {
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            return new Direct3DDevice12Interceptor();
        }

        public override void Dispose()
        {

        }
    }
}