using System;

namespace PixelCapturer
{
    public class CaptureClientProxy : MarshalByRefObject
    {
        public Action Disconnect = () => { };
        public void OnDisconnect()
        {
            Disconnect();
        }
    }
}