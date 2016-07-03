using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PixelCapturer.Logging;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D12.Device;

namespace PixelCapturer.DirectX.Interceptors
{
    public class Direct3DDevice12Interceptor : IDirectXInterceptor
    {
        private readonly ILogger _logger = LoggerFactory.Create<Direct3DDevice12Interceptor>();

        #region Present
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
        #endregion

        #region Present1
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int Present1Delegate(IntPtr swapChainPtr, int syncInterval, PresentFlags flags);

        private Action<SwapChain1> _present1Delegate = device => { };

        private readonly Hook<Present1Delegate> _present1Hook;

        private int Present1Hook(IntPtr swapChainPtr, int syncInterval, PresentFlags flags)
        {
            var swapChain = new SwapChain1(swapChainPtr);
            try
            {
                _present1Delegate(swapChain);
            }
            catch (Exception ex)
            {
                _logger.Log("EXCEPTION: {0}, STACKTRACE: {1}", ex.Message, ex.StackTrace);
            }
            return _present1Hook.Original(swapChainPtr, syncInterval, flags);
        }

        public void OnPresent1(Action<SwapChain1> ev)
        {
            _present1Delegate = ev;
        }
        #endregion

        public Direct3DDevice12Interceptor()
        {

            Dictionary<DxgiSwapChain1Vtbl, IntPtr> dxgiSwapChain1Addresses;
            Device device;
            SwapChain1 swapChain;

            var descriptor = new SwapChainDescription1
            {
                BufferCount = 2,
                Flags = SwapChainFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipDiscard,
                Usage = Usage.RenderTargetOutput,
                Format = Format.R8G8B8A8_UNorm,
                Width = 100,
                Height = 100,
                AlphaMode = AlphaMode.Ignore,
                Scaling = Scaling.None,
                Stereo = false
            };

            CreateDeviceWithSwapChain1(
                FeatureLevel.Level_12_0,
                descriptor,
                out device,
                out swapChain);

            using (swapChain)
            {
                var vTable = Marshal.ReadIntPtr(swapChain.NativePointer);
                dxgiSwapChain1Addresses = Enum
                    .GetValues(typeof(DxgiSwapChain1Vtbl))
                    .Cast<short>()
                    .ToDictionary(index => (DxgiSwapChain1Vtbl)index,
                        index => Marshal.ReadIntPtr(vTable, index * IntPtr.Size));
            }

            _presentHook = new Hook<PresentDelegate>(dxgiSwapChain1Addresses[DxgiSwapChain1Vtbl.Present],
                new PresentDelegate(PresentHook), this);
            _present1Hook = new Hook<Present1Delegate>(dxgiSwapChain1Addresses[DxgiSwapChain1Vtbl.Present1],
                new Present1Delegate(Present1Hook), this);
        }

        private static void CreateDeviceWithSwapChain1(FeatureLevel level, SwapChainDescription1 swapChainDescription,
            out Device device,
            out SwapChain1 swapChain)
        {
            using (var factory = new Factory4())
            {
                device = new Device(null, level);
                using (var form = new RenderForm())
                {
                    var queue = device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));
                    swapChain = new SwapChain1(factory, queue, form.Handle, ref swapChainDescription);
                }
            }
        }

        public void Dispose()
        {
            _presentHook.Dispose();
            _present1Hook.Dispose();
        }
    }
}