using System;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Threading.Tasks;
using EasyHook;
using PixelCapturer.DirectX;
using PixelCapturer.DirectX.Detectors;
using PixelCapturer.DirectX.Handlers;
using PixelCapturer.Logging;

namespace PixelCapturer
{
    public class InjectionEntry : IEntryPoint
    {
        private readonly CaptureClient _client;
        private ManualResetEvent _reset;
        private readonly CaptureClientProxy _clientProxy;
        private IpcServerChannel _clientServerChannel;

        private static ILogger Logger => new Lazy<ILogger>(LoggerFactory.Create<InjectionEntry>).Value;

        public InjectionEntry(RemoteHooking.IContext context, string channelName)
        {
            SetupClientServerChannel(channelName);

            _client = RemoteHooking.IpcConnectClient<CaptureClient>(channelName);

            LoggerFactory.Set(type => new TypeLoggerDecorator(type, _client));

            _clientProxy = new CaptureClientProxy
            {
                Disconnect = Disconnect
            };
            _client.OnDisconnect += _clientProxy.OnDisconnect;
        }

        public void Run(RemoteHooking.IContext context, string channelName)
        {
            Logger.Log("Connected to target process with id {0}", RemoteHooking.GetCurrentProcessId());

            _reset = new ManualResetEvent(false);

            var colorMapper = new ColorMapper(new PrevColorHolder(), new GammaCorrection());
            var pixelCalculator = new PixelCalculator();

            var directXLoader = new DirectXLoader(
                new DirectXD3D9Detector(new D3D9PixelHandler(_client, colorMapper, pixelCalculator)),
                new DirectXD3D11Detector(new D3D11PixelHandler(_client, colorMapper, pixelCalculator)));

            var directXInterceptors = directXLoader.Load();

            StartPeriodPinging();

            Logger.Log("Waiting...");
            _reset.WaitOne();

            foreach (var directXInterceptor in directXInterceptors)
            {
                directXInterceptor.Dispose();
            }
            directXLoader.Dispose();

            Logger.Log("Quiting.");
            System.Runtime.Remoting.Channels.ChannelServices.UnregisterChannel(_clientServerChannel);
            Thread.Sleep(1000);
        }

        private void SetupClientServerChannel(string channelName)
        {
            var sinkProvider = new System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full
            };
            _clientServerChannel = new IpcServerChannel(channelName, channelName + Guid.NewGuid().ToString("N"), sinkProvider);
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(_clientServerChannel, false);
        }

        private void Disconnect()
        {
            Logger.Log("Resetting");
            _reset.Set();
        }

        private void StartPeriodPinging()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (_reset.WaitOne(0))
                    {
                        _client.Ping();
                        Thread.Sleep(1000);
                    }

                }
                catch
                {
                    _reset.Set();
                }
            });
        }
    }
}
