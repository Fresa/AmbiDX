using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PixelCapturer.Logging;
using SharpDX.Direct3D9;

namespace PixelCapturer.DirectX.Interceptors
{
    public class Direct3DDevice9Interceptor : IDirectXInterceptor
    {
        private readonly ILogger _logger = LoggerFactory.Create<Direct3DDevice9Interceptor>();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int EndSceneDelegate(IntPtr device);
        private Action<Device> _endSceneDelegate = device => { };
        private readonly Hook<EndSceneDelegate> _endSceneHook;
        
        public Direct3DDevice9Interceptor()
        {
            Dictionary<Direct3DDevice9FunctionOrdinals, IntPtr> direct3DDevice9Addresses = null;
            using (var d3D = new Direct3D())
            {
                using (var renderForm = new Form())
                {
                    using (var device = new Device(d3D, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderForm.Handle }))
                    {
                        var vTable = Marshal.ReadIntPtr(device.NativePointer);
                        direct3DDevice9Addresses = Enum
                            .GetValues(typeof(Direct3DDevice9FunctionOrdinals))
                            .Cast<short>()
                            .ToDictionary(index => (Direct3DDevice9FunctionOrdinals)index,
                                index => Marshal.ReadIntPtr(vTable, index * IntPtr.Size));
                    }
                }
            }

            _endSceneHook = new Hook<EndSceneDelegate>(direct3DDevice9Addresses[Direct3DDevice9FunctionOrdinals.EndScene], new EndSceneDelegate(EndSceneHook), this);
        }

        private int EndSceneHook(IntPtr devicePtr)
        {
            var device = new Device(devicePtr);
            try
            {
                _endSceneDelegate(device);
            }
            catch (Exception ex)
            {
                _logger.Log("EXCEPTION: {0}, STACKTRACE: {1}", ex.Message, ex.StackTrace);
            }
            return _endSceneHook.Original(devicePtr);
        }

        public void OnEndScene(Action<Device> ev)
        {
            _endSceneDelegate = ev;
        }

        
        public void Dispose()
        {
            _endSceneHook.Dispose();
        }
    }
}