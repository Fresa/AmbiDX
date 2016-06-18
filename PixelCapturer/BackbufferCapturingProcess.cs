using System;
using System.Runtime.Remoting;
using System.Threading;
using EasyHook;
using PixelCapturer.LightsConfiguration;

namespace PixelCapturer
{
    public class BackbufferCapturingProcess : ICapturingProcess
    {
        private readonly LightConfiguration _lightConfiguration;
        private CaptureClient _client;
        private Action<int[,]> _onCapturing = rectangle => { };

        public BackbufferCapturingProcess(LightConfiguration lightConfiguration)
        {
            _lightConfiguration = lightConfiguration;
            if (AppConfiguration.AutoRegisterInGac)
            {
                Config.Register(typeof(CaptureClient).Assembly.GetName().Name,
                    typeof(CaptureClient).Assembly.Location);
            }
        }

        public void Start(int processId)
        {
            _client = new CaptureClient();
            _client.OnDataStreaming += rectangle => _onCapturing(rectangle);

            string channelName = null;
            RemoteHooking.IpcCreateServer(
                ref channelName, WellKnownObjectMode.Singleton, _client);

            RemoteHooking.Inject(
                processId,
                typeof(CaptureClient).Assembly.Location,
                typeof(CaptureClient).Assembly.Location,
                channelName,
                _lightConfiguration);
        }

        public void Stop()
        {
            try
            {
                _client.Disconnect();
            }
            catch
            {
                // Client already disconnected
            }
        }

        public void OnScreenCaptured(Action<int[,]> ev, CancellationTokenSource cancellationToken)
        {
            _onCapturing = ev;
        }
    }
}