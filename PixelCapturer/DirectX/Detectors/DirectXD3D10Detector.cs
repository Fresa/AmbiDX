using PixelCapturer.DirectX.Interceptors;
using PixelCapturer.Logging;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D10Detector : DirectXDetector
    {
        private const string DirectXDllFileName = "d3d10.dll";
        private readonly ILogger _logger = LoggerFactory.Create<DirectXDetector>();

        public DirectXD3D10Detector() : base(DirectXDllFileName)
        {
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            _logger.Log("DirectX 10 is not supported.");
            return new DummyInterceptor();
        }

        public override void Dispose()
        {
            
        }
    }
}