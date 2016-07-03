using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmbiDX.Settings.Lights;
using AmbiDX.Settings.Preview;
using AmbiDX.Settings.Process;
using AmbiDX.Settings.SerialCommunication;
using PixelCapturer;
using PixelCapturer.LightsConfiguration;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D12.Device;
using Font = System.Drawing.Font;
using Process = System.Diagnostics.Process;

namespace AmbiDX
{
    public partial class Preview : Form
    {
        private readonly CancellationTokenSource _cancel;
        private readonly byte[] _darkLeds;
        private readonly Button _stop;
        private readonly Button _capture;
        private readonly ToolStripStatusLabel _status;
        private readonly ToolStrip _toolstrip;
        private readonly Button _lightsOn;
        private readonly Button _lightsOff;
        private readonly ComboBox _processes;
        private readonly Button _addToWatcher;
        private readonly Button _removeFromWatcher;
        private readonly ListBox _watcher;
        private readonly CheckBox _autoDetectProcesses;
        private NotifyIcon _notifyIcon;

        private AutoResetEvent _serialDataAvailable;

        public Preview()
        {
            InitializeComponent();
            components = new Container();

            StartPosition = FormStartPosition.Manual;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.Black;

            var width = PreviewConfig.Get().PixelSize;
            var height = PreviewConfig.Get().PixelSize;
            const int topOffset = 100;

            var font = new Font(new FontFamily(GenericFontFamilies.SansSerif), (int)Math.Ceiling((double)height / 6));

            _darkLeds = Enumerable.Repeat((byte)0, LightsConfig.LedCount * 3).ToArray();

            _lightsOn = new Button
            {
                Text = @"Lights on",
                BackColor = SystemColors.Control
            };
            _lightsOn.Click += lightsOn_Click;
            Controls.Add(_lightsOn);

            _lightsOff = new Button
            {
                Text = @"Lights off",
                BackColor = SystemColors.Control,
                Enabled = false
            };
            _lightsOff.Click += lightsOff_Click;
            Controls.Add(_lightsOff);

            _capture = new Button
            {
                Text = @"Capture",
                BackColor = SystemColors.Control
            };
            _capture.Click += startCapturing_Click;

            Controls.Add(_capture);

            _stop = new Button
            {
                Text = @"Stop capturing",
                BackColor = SystemColors.Control
            };
            _stop.Click += stop_Click;

            Controls.Add(_stop);

            var processItems = Process.GetProcesses()
                .OrderBy(process => process.ProcessName)
                .Select(process => new ProcessListItem(process))
                .ToList();
            processItems.Insert(0, new ProcessListItem("Screen", -1));

            _processes = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = processItems
            };

            Controls.Add(_processes);

            _addToWatcher = new Button
            {
                Text = @"Add >>",
                BackColor = SystemColors.Control
            };
            _addToWatcher.Click += AddToWatcherOnClick;
            Controls.Add(_addToWatcher);

            _removeFromWatcher = new Button
            {
                Text = @"<< Remove",
                BackColor = SystemColors.Control
            };
            _removeFromWatcher.Click += RemoveFromWatcherOnClick;
            Controls.Add(_removeFromWatcher);

            _watcher = new ListBox();
            _watcher.Items.AddRange(ProcessSettingsHandler.GetProcessNames().Cast<object>().ToArray());
            Controls.Add(_watcher);

            _autoDetectProcesses = new CheckBox
            {
                Checked = true,
                Text = @"Auto detect",
                ForeColor = Color.White
            };
            Controls.Add(_autoDetectProcesses);

            _status = new ToolStripStatusLabel();
            _toolstrip = new ToolStrip(_status)
            {
                Dock = DockStyle.None,
                GripStyle = ToolStripGripStyle.Hidden,
                AllowDrop = false,
                Renderer = new ToolStripProfessionalRenderer { RoundedEdges = false },
                AutoSize = false,
                Width = Width,
                Top = height * LightsConfig.Get().Displays.Max(display => display.Leds.Max(led => led.Row)) + topOffset + height
            };
            Controls.Add(_toolstrip);

            _notifyIcon = new NotifyIcon(components)
            {
                BalloonTipTitle = @"AmbiDX",
                Icon = new Icon("ambidx.ico"),
                Visible = true
            };
            _notifyIcon.DoubleClick += ToggleMinimizeState;
            _notifyIcon.ContextMenu = new ContextMenu(new[] { new MenuItem("Exit", (sender, args) => Application.Exit()) });
            Resize += SetMinimizeState;

            var screenLeft = 0;
            const int screenTop = topOffset;
            foreach (var item in LightsConfig.Get().Displays.Select((display, index) => new { Index = index, Display = display }))
            {
                var screenLabel = new Label
                {
                    Text = $"Screen {item.Index}",
                    Left = screenLeft,
                    ForeColor = Color.White
                };
                Controls.Add(screenLabel);
                screenLabel.Top = screenTop - screenLabel.Height;

                foreach (var led in item.Display.Leds)
                {
                    var screen = item.Index;
                    var left = screenLeft + width * led.Column;
                    var top = screenTop + height * led.Row;

                    Controls.Add(new Label
                    {
                        Name = $"{screen}-{led.Column}-{led.Row}",
                        Text = $"{led.Column},{led.Row}",
                        Font = font,
                        Width = width,
                        Height = height,
                        Left = left,
                        Top = top,
                        ForeColor = Color.White
                    });

                }
                screenLeft += item.Display.Columns * width;
            }

            _cancel = new CancellationTokenSource();

            if (_autoDetectProcesses.Checked)
            {
                StartDetectingWatchedProcesses();
            }

            //Test();
        }

        private void ToggleMinimizeState(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Minimized ? FormWindowState.Normal : FormWindowState.Minimized;
        }

        private void SetMinimizeState(object sender, EventArgs e)
        {
            var isMinimized = WindowState == FormWindowState.Minimized;

            ShowInTaskbar = !isMinimized;
            if (isMinimized)
            {
                _notifyIcon.ShowBalloonTip(500, "AmbiDX", "...is still running in the background", ToolTipIcon.Info);
            }
        }

        private void RemoveFromWatcherOnClick(object sender, EventArgs eventArgs)
        {
            ProcessSettingsHandler.Delete(new ProcessElement { Name = _watcher.SelectedItem.ToString() });
            _watcher.Items.Remove(_watcher.SelectedItem);
        }

        private void AddToWatcherOnClick(object sender, EventArgs eventArgs)
        {
            var selectedProcess = ((ProcessListItem)_processes.SelectedItem).Name;
            if (_watcher.Items.Cast<string>().Contains(selectedProcess, StringComparer.CurrentCultureIgnoreCase))
            {
                return;
            }
            _watcher.Items.Add(selectedProcess);
            ProcessSettingsHandler.Save(new ProcessElement { Name = selectedProcess });
        }

        public override sealed bool AutoSize
        {
            get { return base.AutoSize; }
            set { base.AutoSize = value; }
        }

        public override sealed Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        private static int CalculateMaxWidth(ListControl list, IEnumerable items)
        {
            var maxItemWidth = (from object item in items select TextRenderer.MeasureText(list.GetItemText(item), list.Font).Width)
                .Concat(new[] { 100 })
                .Max();
            return maxItemWidth + SystemInformation.VerticalScrollBarWidth;
        }

        protected override void OnLoad(EventArgs e)
        {
            var screen = Screen.AllScreens.FirstOrDefault(s => s.Primary == false) ?? Screen.PrimaryScreen;
            Location = new Point(screen.Bounds.Location.X + (screen.WorkingArea.Width - Width) / 2, screen.Bounds.Location.Y + (screen.WorkingArea.Height - Height) / 2);
            _toolstrip.Width = Width;

            var maxWidthButtonSize = Width / 4;
            var maxHeightButtonSize = Height / 4;
            var buttonWidth = 100 > maxWidthButtonSize ? maxWidthButtonSize : 100;
            var buttonHeight = 20 > maxHeightButtonSize ? maxHeightButtonSize : 20;
            var buttonLeft = 0;
            var buttonTop = 0;
            const int buttonXOffset = 10;
            const int buttonYOffset = 10;

            // First column of controllers
            _lightsOn.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));
            buttonTop += buttonHeight + buttonYOffset;
            _lightsOff.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));

            buttonLeft += buttonWidth + buttonXOffset;
            buttonTop = 0;

            // Second column of controllers
            _capture.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));
            buttonTop += buttonHeight + buttonYOffset;
            _stop.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));

            buttonLeft += buttonWidth + buttonXOffset;
            buttonTop = 0;

            // Third column
            _processes.DropDownWidth = _processes.Width = CalculateMaxWidth(_processes, _processes.Items);
            _processes.Location = new Point(buttonLeft, buttonTop);

            buttonLeft += _processes.DropDownWidth + buttonXOffset;
            buttonTop = 0;

            // Fourth column
            _addToWatcher.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));
            buttonTop += buttonHeight + buttonYOffset;
            _removeFromWatcher.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));

            buttonLeft += buttonWidth + buttonXOffset;
            buttonTop = 0;

            // Fifth column
            _watcher.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(CalculateMaxWidth(_watcher, _watcher.Items), 200));
            _watcher.Height = _removeFromWatcher.Bottom;

            buttonLeft += _watcher.Width + buttonXOffset;
            buttonTop = 0;

            // Sixth column
            _autoDetectProcesses.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(buttonWidth, buttonHeight));

            base.OnLoad(e);
        }

        private IEnumerable<ICapturingProcess> _screenCapturingProcesses = new List<ICapturingProcess>();

        private void startCapturing_Click(object sender, EventArgs e)
        {
            _autoDetectProcesses.Checked = false;
            StartCapture(((ProcessListItem)_processes.SelectedItem).Id);
        }

        private void StartCapture(int processId)
        {
            lock (_capturingLock)
            {
                if (_capturing)
                {
                    return;
                }
                _capturing = true;
            }

            if (processId == -1)
            {
                UpdateStatus(@"Starting capturing front buffer");
                _screenCapturingProcesses = LightsConfig.Get().Displays.Select((display, displayIndex) => new FrontbufferCapturer(displayIndex, LightsConfig.ToLightConfiguration())).ToList();
            }
            else
            {
                UpdateStatus($"Starting capturing back buffer from process with id {processId}");
                _screenCapturingProcesses = new[] { new BackbufferCapturingProcess(LightsConfig.ToLightConfiguration()) };
            }

            _stop.Invoke(stop => stop.Enabled = true);
            _capture.Invoke(capture => capture.Enabled = false);

            _serialDataAvailable = new AutoResetEvent(false);
            _ledDataAvailable = new AutoResetEvent(false);

            StartSerialCommunicationTask();
            StartUpdatingPreviewTask();

            //decimal avg = 0;

            foreach (var screenCapture in _screenCapturingProcesses)
            {
                screenCapture.OnScreenCaptured(ledColors =>
                {
                    //var watch = Stopwatch.StartNew();
                    var serialData = (from int ledColor in ledColors select Convert.ToByte(ledColor)).ToArray();

                    _serialLock.EnterWriteLock();
                    _serialData = serialData;
                    _serialLock.ExitWriteLock();
                    _serialDataAvailable.Set();

                    _ledDataLock.EnterWriteLock();
                    _ledData = ledColors;
                    _ledDataLock.ExitWriteLock();
                    _ledDataAvailable.Set();

                    //var time = watch.ElapsedMilliseconds;

                    //decimal localAvg;
                    //decimal fps;
                    //lock (_avgLock)
                    //{
                    //    if (avg == 0)
                    //    {
                    //        avg = time;
                    //    }

                    //    avg = Math.Round(new[] { avg, time }.Average(), 2);
                    //    localAvg = avg;
                    //    fps = Math.Round(1000 / avg, 0);
                    //}

                    //MethodInvoker updateStatus = delegate
                    //{
                    //    Interlocked.Increment(ref _numberOfScreenCaptures);
                    //    _status.Text = $"{_numberOfScreenCaptures}: Elapsed time: {time}. Average: {localAvg}. FPS: {fps}";
                    //};
                    //Invoke(updateStatus);
                }, _cancel);

                screenCapture.Start(processId);
            }
        }

        readonly ReaderWriterLockSlim _serialLock = new ReaderWriterLockSlim();
        private byte[] _serialData = new byte[0];

        //readonly object _avgLock = new object();

        readonly ReaderWriterLockSlim _ledDataLock = new ReaderWriterLockSlim();
        private int[,] _ledData;
        private AutoResetEvent _ledDataAvailable;

        private bool _capturing;
        private readonly object _capturingLock = new object();
        private CommandQueue _commandQueue;
        private Fence _fence;
        private int _fenceValue;
        private AutoResetEvent _fenceEvent;

        private void StartDetectingWatchedProcesses()
        {
            var detectingWachedProcessesTask = new Task(() =>
            {
                UpdateStatus(@"Searching for processes...");

                while (true)
                {
                    Thread.Sleep(100);
                    if (_cancel.Token.IsCancellationRequested)
                    {
                        UpdateStatus("");
                        return;
                    }

                    var watchedProcesses = _watcher.Invoke(watcher => watcher.Items.Cast<string>().ToArray());
                    var watchedProcessesFoundRunning = Process.GetProcesses().Select(process => process.ProcessName).Intersect(watchedProcesses, StringComparer.CurrentCultureIgnoreCase).ToList();
                    if (watchedProcessesFoundRunning.Any())
                    {
                        var capturingProcess = watchedProcessesFoundRunning.First();
                        var processId = Process.GetProcesses().First(process => process.ProcessName.Equals(capturingProcess, StringComparison.CurrentCultureIgnoreCase)).Id;

                        UpdateStatus($"Found process {capturingProcess} with id {processId}, starting capturing.");
                        StartCapture(processId);
                        return;
                    }
                }
            }, TaskCreationOptions.LongRunning);
            detectingWachedProcessesTask.Start();
        }

        private void StartUpdatingPreviewTask()
        {
            var updateLedUiTask = new Task(() =>
            {
                while (true)
                {
                    _ledDataAvailable.WaitOne();
                    if (_cancel.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    _ledDataLock.EnterReadLock();
                    var ledColors = new int[_ledData.GetLength(0), _ledData.GetLength(1)];
                    Buffer.BlockCopy(_ledData, 0, ledColors, 0, _ledData.Length * 4);
                    _ledDataLock.ExitReadLock();

                    this.Invoke(preview => DrawPreviewLeds(preview, ledColors));
                }
            }, TaskCreationOptions.LongRunning);
            updateLedUiTask.Start();
        }

        private static void DrawPreviewLeds(Control ledContainer, int[,] colors)
        {
            var leds =
                LightsConfig.Get()
                    .Displays.SelectMany(
                        (display, i) =>
                            display.Leds.Select(led => new { Screen = i, led.Column, led.Row })).ToArray();

            for (var i = colors.GetLength(0) - 1; i >= 0; i--)
            {
                var ledColor = Color.FromArgb(255, colors[i, 0], colors[i, 1], colors[i, 2]);
                var label = ledContainer.Controls.Find($"{leds[i].Screen}-{leds[i].Column}-{leds[i].Row}", false).First();
                label.BackColor = ledColor;
                label.ForeColor = Color.FromArgb(255, (byte)(colors[i, 0] + 128),
                    (byte)(colors[i, 1] + 128), (byte)(colors[i, 2] + 128));
            }
        }

        private void StartSerialCommunicationTask()
        {
            var serialCommuncationTask = new Task(() =>
            {
                ISerialWriter serialWriter;
                if (SerialCommunicationConfig.Get().Enabled)
                {
                    serialWriter = new AdaLedSerialWriter();
                }
                else
                {
                    serialWriter = new DummySerialWriter();
                }

                while (true)
                {
                    _serialDataAvailable.WaitOne();
                    if (_cancel.Token.IsCancellationRequested)
                    {
                        serialWriter.Write(_darkLeds);
                        serialWriter.Dispose();
                        return;
                    }

                    _serialLock.EnterReadLock();
                    serialWriter.Write(_serialData);
                    _serialLock.ExitReadLock();

                }
            }, TaskCreationOptions.LongRunning);
            serialCommuncationTask.Start();
        }

        private void stop_Click(object sender, EventArgs e)
        {
            _stop.Enabled = false;
            _cancel?.Cancel();
            foreach (var screenCapturingProcess in _screenCapturingProcesses)
            {
                screenCapturingProcess.Stop();
            }
            _serialDataAvailable?.Set();
            _ledDataAvailable?.Set();
            _capture.Enabled = true;

            _autoDetectProcesses.Checked = false;
            lock (_capturingLock)
            {
                _capturing = false;
            }
        }

        private void lightsOn_Click(object sender, EventArgs e)
        {
            _lightsOn.Enabled = false;
            var serialWriter = new AdaLedSerialWriter();
            var random = new Random();
            var colors = new byte[LightsConfig.LedCount * 3];
            for (var i = 0; i < LightsConfig.LedCount * 3; i++)
            {
                colors[i] = (byte)random.Next(0, 255);
            }
            serialWriter.Write(colors);
            serialWriter.Dispose();
            _lightsOff.Enabled = true;
        }

        private void lightsOff_Click(object sender, EventArgs e)
        {
            _lightsOff.Enabled = false;
            var serialWriter = new AdaLedSerialWriter();
            serialWriter.Write(_darkLeds);
            serialWriter.Dispose();
            _lightsOn.Enabled = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                return;
            }

            _cancel?.Cancel(false);
            Application.DoEvents();
            OnClosing(e);
        }

        private void UpdateStatus(string status)
        {
            _toolstrip.Invoke(toolStrip => _status.Text = status);
        }

        private SwapChain3 LoadPipeline()
        {
            var form = new RenderForm();
            int width = form.ClientSize.Width;
            int height = form.ClientSize.Height;


            var device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_0);
            SwapChain3 swapChain;
            using (var factory = new Factory4())
            {
                // Describe and create the command queue.
                var queueDesc = new CommandQueueDescription(CommandListType.Direct);
                var commandQueue = device.CreateCommandQueue(queueDesc);


                // Describe and create the swap chain.
                var swapChainDesc = new SwapChainDescription()
                {
                    BufferCount = 2,
                    ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                    Usage = Usage.RenderTargetOutput,
                    SwapEffect = SwapEffect.FlipDiscard,
                    OutputHandle = form.Handle,
                    //Flags = SwapChainFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                    IsWindowed = true
                };

                var tempSwapChain = new SwapChain(factory, commandQueue, swapChainDesc);
                swapChain = tempSwapChain.QueryInterface<SwapChain3>();
                tempSwapChain.Dispose();
            }

            // Create descriptor heaps.
            // Describe and create a render target view (RTV) descriptor heap.
            var rtvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = 2,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };

            var renderTargetViewHeap = device.CreateDescriptorHeap(rtvHeapDesc);

            var srvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = 1,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            };

            var shaderRenderViewHeap = device.CreateDescriptorHeap(srvHeapDesc);

            var rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

            var resource = device.CreateCommittedResource(new HeapProperties(HeapType.Default)
            {
                CPUPageProperty = CpuPageProperty.Unknown,
                MemoryPoolPreference = MemoryPool.Unknown,
                CreationNodeMask = 1,
                VisibleNodeMask = 1
            }, HeapFlags.None,
                ResourceDescription.Texture2D(Format.B8G8R8A8_UNorm, 100, 100),
                ResourceStates.CopyDestination,
                null);

            // Create frame resources.
            var rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            var renderTargets = swapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(0);
            device.CreateRenderTargetView(renderTargets, null, rtvHandle);
            device.CreateRenderTargetView(resource, null, rtvHandle);
            return swapChain;
        }

        private void Test()
        {
            DebugInterface.Get().EnableDebugLayer();

            var swapChain = LoadPipeline();
            
            TextureCopyLocation destLocation;
            TextureCopyLocation srcLocation;
            using (var backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(0))
            {
                var description = swapChain.Description;
                var description1 = new SwapChainDescription1
                {
                    BufferCount = description.BufferCount,
                    Flags = description.Flags,
                    SampleDescription = description.SampleDescription,
                    SwapEffect = description.SwapEffect,
                    Usage = description.Usage,
                    Format = description.ModeDescription.Format,
                    Width = description.ModeDescription.Width,
                    Height = description.ModeDescription.Height,
                    AlphaMode = AlphaMode.Ignore,
                    Scaling = Scaling.None,
                    Stereo = false
                };

                Device device;
                using (var factory = new Factory4())
                {
                    device = new Device(null, FeatureLevel.Level_12_0);
                    _commandQueue = device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));

                    //using (var form = new RenderForm())
                    //{
                    //    var sc = new SwapChain1(factory, _commandQueue, form.Handle, ref description1);
                    //}
                }

                PlacedSubResourceFootprint[] footprints = { new PlacedSubResourceFootprint() };
                var buffRes = backBuffer.Description;
                long bufftotalBytes;
                device.GetCopyableFootprints(ref buffRes, 0, 1, 0, footprints, null, null, out bufftotalBytes);

                var readBackBufferDescription = ResourceDescription.Buffer(new ResourceAllocationInformation
                {
                    Alignment = 0,
                    SizeInBytes = bufftotalBytes
                });

                var screenTexture = device.CreateCommittedResource(
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
                
                destLocation = new TextureCopyLocation(screenTexture, new PlacedSubResourceFootprint {Footprint = footprints[0].Footprint });
                srcLocation = new TextureCopyLocation(backBuffer, 0);

                var commandAllocator = device.CreateCommandAllocator(CommandListType.Direct);
                var commandList = device.CreateCommandList(CommandListType.Direct, commandAllocator, null);

                //commandList.ResourceBarrierTransition(backBuffer, ResourceStates.Present,
                //    ResourceStates.CopySource);

                commandList.CopyTextureRegion(destLocation, 0, 0, 0, srcLocation, null);
                //commandList.ResourceBarrierTransition(backBuffer, ResourceStates.CopySource,
                //    ResourceStates.Present);

                commandList.Close();
                _commandQueue.ExecuteCommandList(commandList);

                _fence = device.CreateFence(0, FenceFlags.None);
                _fenceValue = 1;

                _fenceEvent = new AutoResetEvent(false);

                WaitForPreviousFrame();

                var pnt = screenTexture.Map(0);

                var dataRectangle = new DataRectangle
                {
                    DataPointer = pnt,
                    Pitch = footprints[0].Footprint.RowPitch
                };

                var dataStream = new DataStream(new DataPointer(pnt, (int)bufftotalBytes));

                var pixels = new List<byte[]>();
                for (var y = 0; y < backBuffer.Description.Height; y++)
                {
                    for (var x = 0; x < backBuffer.Description.Width; x++)
                    {
                        dataStream.Seek((y * dataRectangle.Pitch) + (x * 4), SeekOrigin.Begin);
                        var buffer = new byte[4];
                        dataStream.Read(buffer, 0, 4);
                        pixels.Add(buffer);
                    }
                }
                
                screenTexture.Unmap(0);

                //_client.StreamData(pixelData);
            }
        }

        private void WaitForPreviousFrame()
        {
            // WAITING FOR THE FRAME TO COMPLETE BEFORE CONTINUING IS NOT BEST PRACTICE. 
            // This is code implemented as such for simplicity. 

            int localFence = _fenceValue;
            _commandQueue.Signal(this._fence, localFence);
            _fenceValue++;

            // Wait until the previous frame is finished.
            if (_fence.CompletedValue < localFence)
            {
                _fence.SetEventOnCompletion(localFence, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
                _fenceEvent.WaitOne();
            }

            //frameIndex = swapChain.CurrentBackBufferIndex;
        }
    }
}