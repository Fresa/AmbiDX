using PixelCapturer.DirectX.Handlers;
using PixelCapturer.DirectX.Interceptors;
using PixelCapturer.Logging;

namespace PixelCapturer.DirectX.Detectors
{
    public class DirectXD3D12Detector : DirectXDetector
    {
        private readonly IDirect3DDevice12Handler _handler;
        private const string DirectXDllFileName = "d3d12.dll";
        private readonly ILogger _logger = LoggerFactory.Create<DirectXD3D12Detector>();

        public DirectXD3D12Detector(IDirect3DDevice12Handler handler) : base(DirectXDllFileName)
        {
            _handler = handler;
        }

        protected override IDirectXInterceptor DirectXInterceptorFactory()
        {
            var interceptor = new Direct3DDevice12Interceptor();
            interceptor.OnPresent1(_handler.Present1Delegate);
            interceptor.OnPresent(_handler.PresentDelegate);
            _logger.Log("Subscribed");
            return interceptor;
        }

        public override void Dispose()
        {
            _handler.Dispose();
        }
    }
}