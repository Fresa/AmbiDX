using System;
using System.Windows.Forms;

namespace AmbiDX
{
    public static class ControlExtensions
    {
        public static TResult Invoke<TControl, TResult>(this TControl control, Func<TControl, TResult> func)
            where TControl : Control
        {
            if (control.InvokeRequired)
            {
                return (TResult) control.Invoke(func, control);
            }
            return func(control);
        }

        public static void Invoke<TControl>(this TControl control, Action<TControl> action)
            where TControl : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action, control);
            }
            else
            {
                action(control);
            }
        }
    }
}