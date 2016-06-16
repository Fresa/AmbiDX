using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Security.Principal;
using System.Threading;
using EasyHook;
using PixelCapturer.LightsConfiguration;
using SharpDX;

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
        }

        public void Start(int processId)
        {
            _client = new CaptureClient();
            _client.OnDataStreaming += rectangle => _onCapturing(rectangle);
            EasyHook.Config.Register("PixelCapturer",
                typeof(CaptureClient).Assembly.Location);

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
            }
        }

        public void OnScreenCaptured(Action<int[,]> ev, CancellationTokenSource cancellationToken)
        {
            _onCapturing = ev;
        }
    }
}