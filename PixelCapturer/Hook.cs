using System;
using System.Runtime.InteropServices;
using EasyHook;

namespace PixelCapturer
{
    public class Hook<TDelegate> : IDisposable
        where TDelegate : class
    {
        private readonly LocalHook _hook;
        public TDelegate Original { get; private set; }

        public Hook(IntPtr hookPointer, Delegate hookingDelegate, object callback)
        {
            _hook = LocalHook.Create(hookPointer, hookingDelegate, callback);
            Original = (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(hookPointer, typeof(TDelegate));
            _hook.ThreadACL.SetExclusiveACL(new[] { 0 });
        }

        public void Dispose()
        {
            _hook.Dispose();
        }
    }
}