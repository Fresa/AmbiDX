using System;
using System.Runtime.InteropServices;

namespace PixelCapturer
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}