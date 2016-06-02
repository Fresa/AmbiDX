using System;
using PixelCapturer.Logging;
using SharpDX;

namespace PixelCapturer
{
    [Serializable]
    public delegate void DisconnectedEvent();

    [Serializable]
    public delegate void PixelDataEvent(int[,] data);

    [Serializable]
    public class CaptureClient : MarshalByRefObject, ILogger
    {
        public void Log(string message, params object[] parameters)
        {
            Console.WriteLine(message, parameters);
        }

        public event PixelDataEvent OnDataStreaming;

        public void StreamData(int[,] pixelData)
        {
            OnDataStreaming?.Invoke(pixelData);
        }

        public void Ping()
        {

        }

        public event DisconnectedEvent OnDisconnect;

        public virtual void Disconnect()
        {
            OnDisconnect?.Invoke();
        }
    }
}