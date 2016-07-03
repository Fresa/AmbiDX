using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using PixelCapturer.Logging;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D12.Device;
using Resource = SharpDX.Direct3D12.Resource;

namespace PixelCapturer.DirectX.Handlers
{
    public class D3D12PixelHandler : IDirect3DDevice12Handler
    {
        private readonly CaptureClient _client;
        private readonly ColorMapper _colorMapper;
        private readonly PixelCalculator _pixelCalculator;
        private readonly object _disposedLock = new object();
        private readonly ILogger _logger = LoggerFactory.Create<D3D12PixelHandler>();
        private Fence _fence;
        private int _fenceValue;
        private AutoResetEvent _fenceEvent;
        private CommandQueue _commandQueue;

        public D3D12PixelHandler(CaptureClient client, ColorMapper colorMapper, PixelCalculator pixelCalculator)
        {
            _client = client;
            _colorMapper = colorMapper;
            _pixelCalculator = pixelCalculator;
        }

        public void PresentDelegate(SwapChain swapChain)
        {
            try
            {
                if (Monitor.TryEnter(_disposedLock) == false)
                {
                    return;
                }

                using (var backBuffer = swapChain.GetBackBuffer<Resource>(0))
                {
                    var display = new Display
                    {
                        Height = backBuffer.Description.Height,
                        Width = (int)backBuffer.Description.Width
                    };
                    var pixelOffset = _pixelCalculator.Calculate(display);

                    var device = new Device(null, FeatureLevel.Level_12_0);
                    _commandQueue = device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));

                    PlacedSubResourceFootprint[] footprints = { new PlacedSubResourceFootprint() };
                    long bufftotalBytes;
                    var description = backBuffer.Description;
                    device.GetCopyableFootprints(ref description, 0, 1, 0, footprints, null, null, out bufftotalBytes);

                    var srcLocation = new TextureCopyLocation(backBuffer, 0);

                    var readBackBufferDescription = ResourceDescription.Buffer(new ResourceAllocationInformation
                    {
                        Alignment = 0,
                        SizeInBytes = bufftotalBytes
                    });

                    var readBackBuffer = device.CreateCommittedResource(
                        new HeapProperties(HeapType.Readback)
                        {
                            CPUPageProperty = CpuPageProperty.Unknown,
                            MemoryPoolPreference = MemoryPool.Unknown,
                            CreationNodeMask = 1,
                            VisibleNodeMask = 1
                        },
                        HeapFlags.None,
                        readBackBufferDescription,
                        ResourceStates.CopyDestination,
                        null);

                    var destLocation = new TextureCopyLocation(readBackBuffer,
                        new PlacedSubResourceFootprint { Footprint = footprints[0].Footprint });

                    var commandAllocator = device.CreateCommandAllocator(CommandListType.Direct);
                    var commandList = device.CreateCommandList(CommandListType.Direct, commandAllocator, null);

                    commandList.CopyTextureRegion(destLocation, 0, 0, 0, srcLocation, null);

                    commandList.Close();
                    _commandQueue.ExecuteCommandList(commandList);

                    _fence = device.CreateFence(0, FenceFlags.None);
                    _fenceValue = 1;

                    _fenceEvent = new AutoResetEvent(false);

                    WaitForPreviousFrame();

                    var pnt = readBackBuffer.Map(0);

                    var dataRectangle = new DataRectangle
                    {
                        DataPointer = pnt,
                        Pitch = footprints[0].Footprint.RowPitch
                    };
                    var dataStream = new DataStream(new DataPointer(pnt, (int)bufftotalBytes));

                    var pixelData = _colorMapper.Map(dataStream, dataRectangle, pixelOffset);

                    readBackBuffer.Unmap(0);

                    _client.StreamData(pixelData);

                    device.Dispose();
                    readBackBuffer.Dispose();
                    _commandQueue.Dispose();
                }

                _fence.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Log(
                    $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException?.Message}\nData: {ex.Data.ToString(",", "-")}\nSource: {ex.Source}\n");
            }
            finally
            {
                Monitor.Exit(_disposedLock);
            }
        }

        private void WaitForPreviousFrame()
        {
            // WAITING FOR THE FRAME TO COMPLETE BEFORE CONTINUING IS NOT BEST PRACTICE. 
            // This is code implemented as such for simplicity. 

            var localFence = _fenceValue;
            _commandQueue.Signal(_fence, localFence);
            _fenceValue++;

            // Wait until the previous frame is finished.
            if (_fence.CompletedValue < localFence)
            {
                _fence.SetEventOnCompletion(localFence, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
                _fenceEvent.WaitOne();
            }
        }

        public void Present1Delegate(SwapChain1 swapChain)
        {
            //try
            //{
            //    if (_first1)
            //    {
            //        _logger.Log("Present1 fired");
            //        _first1 = false;
            //    }

            //    if (Monitor.TryEnter(_disposedLock) == false)
            //    {
            //        _logger.Log("Could not enter lock");
            //        return;
            //    }

            //    TextureCopyLocation destLocation;
            //    TextureCopyLocation srcLocation;
            //    _logger.Log("About to fetch back buffer");

            //    using (var backBuffer = swapChain.GetBackBuffer<Resource>(0))
            //    {
            //        var display = new Display
            //        {
            //            Height = backBuffer.Description.Height,
            //            Width = (int)backBuffer.Description.Width
            //        };

            //        CreateResources(display, swapChain.Description1);

            //        destLocation = new TextureCopyLocation(_screenTexture, 0);

            //        srcLocation = new TextureCopyLocation(backBuffer, 0);
            //    }
            //    var rootSignatureDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout);
            //    var rootSignature = _device.CreateRootSignature(rootSignatureDesc.Serialize());
            //    var inputElementDescs = new[]
            //{
            //        new InputElement("POSITION",0,Format.R32G32B32_Float,0,0),
            //        new InputElement("COLOR",0,Format.R32G32B32A32_Float,12,0)
            //};
            //    var vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "VSMain", "vs_5_0"));
            //    var pixelShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "PSMain", "ps_5_0"));


            //    var pipelineState = _device.CreateGraphicsPipelineState(new GraphicsPipelineStateDescription()
            //    {
            //        InputLayout = new InputLayoutDescription(inputElementDescs),
            //        RootSignature = rootSignature,
            //        VertexShader = vertexShader,
            //        PixelShader = pixelShader,
            //        RasterizerState = RasterizerStateDescription.Default(),
            //        BlendState = BlendStateDescription.Default(),
            //        DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
            //        DepthStencilState = new DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
            //        SampleMask = int.MaxValue,
            //        PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
            //        RenderTargetCount = 1,
            //        Flags = PipelineStateFlags.None,
            //        SampleDescription = new SampleDescription(1, 0),
            //        StreamOutput = new StreamOutputDescription()
            //    });
            //    var commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);
            //    var commandList = _device.CreateCommandList(CommandListType.Direct, commandAllocator, pipelineState);

            //    commandList.ResourceBarrierTransition(_screenTexture, ResourceStates.GenericRead,
            //        ResourceStates.CopyDestination);
            //    commandList.CopyTextureRegion(destLocation, 0, 0, 0, srcLocation, null);
            //    commandList.ResourceBarrierTransition(_screenTexture, ResourceStates.CopyDestination,
            //        ResourceStates.GenericRead);

            //    commandList.Close();
            //    _commandQueue.ExecuteCommandList(commandList);

            //    var pnt = _screenTexture.Map(0);
            //    var dataRectangle = new DataRectangle
            //    {
            //        DataPointer = pnt,
            //        Pitch = (int)_screenTexture.Description.Width * 4
            //    };
            //    var dataStream = new DataStream(new DataPointer(pnt, _screenTexture.Description.DepthOrArraySize));

            //    var pixelData = _colorMapper.Map(dataStream, dataRectangle, _pixelOffset);
            //    _screenTexture.Unmap(0);

            //    _client.StreamData(pixelData);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Log($"Exception: {ex.Message}. StackTrace: {ex.StackTrace}. InnerException:{ex.InnerException?.InnerException.Message}");
            //}
            //finally
            //{
            //    Monitor.Exit(_disposedLock);
            //}
        }

        public void Dispose()
        {
            Monitor.Enter(_disposedLock);
            //_display = null;
            //_device?.Dispose();
            //_readBackBuffer?.Dispose();
            _commandQueue?.Dispose();
        }

    }

    public static class DictionaryExtensions
    {
        public static string ToString(this IDictionary source, string keyValueSeparator,
            string sequenceSeparator)
        {
            if (source == null)
                throw new ArgumentException("Parameter source can not be null.");

            return source.Cast<DictionaryEntry>()
                .Aggregate(new StringBuilder(),
                    (sb, x) => sb.Append(x.Key + keyValueSeparator + x.Value
                                         + sequenceSeparator),
                    sb => sb.ToString(0, (sb.Length == 0 ? 0 : sb.Length - 1)));
        }
    }
}