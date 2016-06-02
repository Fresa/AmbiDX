using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PixelCapturer.Logging;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.DXGI.Device;

namespace PixelCapturer.DirectX.Interceptors
{
    public class Direct3DDevice11Interceptor : IDirectXInterceptor
    {
        private readonly ILogger _logger = LoggerFactory.Create<Direct3DDevice11Interceptor>();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int PresentDelegate(IntPtr swapChainPtr, int syncInterval, PresentFlags flags);

        private Action<SwapChain> _presentDelegate = device => { };

        private readonly Hook<PresentDelegate> _presentHook;

        private int PresentHook(IntPtr swapChainPtr, int syncInterval, PresentFlags flags)
        {
            var swapChain = new SwapChain(swapChainPtr);
            try
            {
                _presentDelegate(swapChain);
            }
            catch (Exception ex)
            {
                _logger.Log("EXCEPTION: {0}, STACKTRACE: {1}", ex.Message, ex.StackTrace);
            }
            return _presentHook.Original(swapChainPtr, syncInterval, flags);
        }

        public void OnPresent(Action<SwapChain> ev)
        {
            _presentDelegate = ev;
        }

        public Direct3DDevice11Interceptor()
        {
            Dictionary<DxgiSwapChainFunctionOrdinals, IntPtr> dxgiSwapChainAddresses;
            using (var renderForm = new RenderForm())
            {
                SharpDX.Direct3D11.Device device;
                SwapChain swapChain;

                SharpDX.Direct3D11.Device.CreateWithSwapChain(
                    DriverType.Hardware,
                    DeviceCreationFlags.BgraSupport,
                    new SwapChainDescription
                    {
                        BufferCount = 1,
                        Flags = SwapChainFlags.None,
                        IsWindowed = true,
                        ModeDescription =
                            new ModeDescription(100, 100, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                        OutputHandle = renderForm.Handle,
                        SampleDescription = new SampleDescription(1, 0),
                        SwapEffect = SwapEffect.Discard,
                        Usage = Usage.RenderTargetOutput
                    },
                    out device,
                    out swapChain);

                using (swapChain)
                {
                    var vTable = Marshal.ReadIntPtr(swapChain.NativePointer);
                    dxgiSwapChainAddresses = Enum
                        .GetValues(typeof(DxgiSwapChainFunctionOrdinals))
                        .Cast<short>()
                        .ToDictionary(index => (DxgiSwapChainFunctionOrdinals)index,
                            index => Marshal.ReadIntPtr(vTable, index * IntPtr.Size));
                }
            }

            _presentHook = new Hook<PresentDelegate>(dxgiSwapChainAddresses[DxgiSwapChainFunctionOrdinals.Present], new PresentDelegate(PresentHook), this);
        }

        public void Dispose()
        {
            _presentHook.Dispose();
        }
    }
}