using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmbiDX.Settings;
using PixelCapturer;
using Font = System.Drawing.Font;
using Process = System.Diagnostics.Process;

namespace AmbiDX
{
    public partial class Preview : Form
    {
        private CancellationTokenSource _cancel;
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

        private ProcessListItem _selectedProcess;
        private AutoResetEvent _serialDataAvailable;

        public Preview()
        {
            StartPosition = FormStartPosition.Manual;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.Black;

            const short width = Config.PixelSize;
            const short height = Config.PixelSize;
            const int topOffset = 100;

            var font = new Font(new FontFamily(GenericFontFamilies.SansSerif), (int)Math.Ceiling((double)height / 6));

            InitializeComponent();

            _darkLeds = Enumerable.Repeat((byte)0, Config.Leds.GetLength(0) * 3).ToArray();

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
            _processes.SelectedValueChanged += (sender, args) =>
            {
                _selectedProcess = (ProcessListItem)_processes.SelectedItem;
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

            _status = new ToolStripStatusLabel();
            _toolstrip = new ToolStrip(_status)
            {
                Dock = DockStyle.None,
                GripStyle = ToolStripGripStyle.Hidden,
                AllowDrop = false,
                Renderer = new ToolStripProfessionalRenderer { RoundedEdges = false },
                AutoSize = false,
                Width = Width,
                Top = height * Config.Leds.Max(ints => ints[2]) + topOffset + height
            };
            Controls.Add(_toolstrip);

            var ledLayouts = Config.Leds
                .GroupBy(ledConfig => ledConfig[0])
                .Select(screen => new
                {
                    Screen = screen.Key,
                    Columns = screen.Min(ints => ints[2]),
                    Rows = screen.Min(ints => ints[1])
                })
                .ToList();

            ledLayouts.ForEach(ledLayout =>
                {
                    var screenLabel = new Label
                    {
                        Text = $"Screen {ledLayout.Screen}",
                        Left = width * ledLayout.Columns,
                        ForeColor = Color.White
                    };

                    Controls.Add(screenLabel);
                    screenLabel.Top = height * ledLayout.Rows + topOffset - screenLabel.Height;
                });

            foreach (var led in Config.Leds)
            {
                var left = width * led[1];
                var top = height * led[2] + topOffset;
                Controls.Add(new Label
                {
                    Name = $"0-{led[1]}-{led[2]}",
                    Text = $"{led[1]},{led[2]}",
                    Font = font,
                    Width = width,
                    Height = height,
                    Left = left,
                    Top = top,
                    ForeColor = Color.White
                });
            }
        }

        private void RemoveFromWatcherOnClick(object sender, EventArgs eventArgs)
        {
            ProcessSettingsHandler.Delete(new ProcessElement { Name = _watcher.SelectedItem.ToString() });
            _watcher.Items.Remove(_watcher.SelectedItem);
        }

        private void AddToWatcherOnClick(object sender, EventArgs eventArgs)
        {
            if (_watcher.Items.Cast<string>().Contains(_selectedProcess.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                return;
            }
            _watcher.Items.Add(_selectedProcess.Name);
            ProcessSettingsHandler.Save(new ProcessElement { Name = _selectedProcess.Name });
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
            Func<ListControl, IEnumerable, int> calculateWidth = (list, items) =>
            {
                var maxItemWidth = (from object item in items select TextRenderer.MeasureText(list.GetItemText(item), list.Font).Width)
                    .Concat(new[] { 100 })
                    .Max();
                return maxItemWidth + SystemInformation.VerticalScrollBarWidth;
            };
            _processes.DropDownWidth = _processes.Width = calculateWidth(_processes, _processes.Items);
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
            _watcher.Bounds = new Rectangle(new Point(buttonLeft, buttonTop), new Size(calculateWidth(_watcher, _watcher.Items), 200));
            _watcher.Height = _removeFromWatcher.Bottom;

            base.OnLoad(e);
        }

        private IEnumerable<ICapturingProcess> _screenCapturingProcesses = new List<ICapturingProcess>();

        private void startCapturing_Click(object sender, EventArgs e)
        {
            if (_selectedProcess.Id == -1)
            {
                _screenCapturingProcesses = Config.Displays.Select(displayConfig => new FrontbufferCapturer(displayConfig[0])).ToList();
            }
            else
            {
                _screenCapturingProcesses = new[] { new BackbufferCapturingProcess() };
            }

            _stop.Enabled = true;
            _capture.Enabled = false;

            _serialDataAvailable = new AutoResetEvent(false);
            _ledDataAvailable = new AutoResetEvent(false);

            StartSerialCommunicationTask();
            StartUpdatingPreviewTask();

            //decimal avg = 0;

            _cancel = new CancellationTokenSource();
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

                screenCapture.Start(_selectedProcess.Id);
            }
        }

        readonly ReaderWriterLockSlim _serialLock = new ReaderWriterLockSlim();
        private byte[] _serialData = new byte[0];

        //readonly object _avgLock = new object();

        readonly ReaderWriterLockSlim _ledDataLock = new ReaderWriterLockSlim();
        private int[,] _ledData;
        private AutoResetEvent _ledDataAvailable;

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

                    MethodInvoker updateLedUi = delegate
                    {
                        //var colors = new List<Color>();
                        for (var i = ledColors.GetLength(0) - 1; i >= 0; i--)
                        {
                            var ledColor = Color.FromArgb(255, ledColors[i, 0], ledColors[i, 1], ledColors[i, 2]);
                            //  colors.Add(ledColor);
                            var col = Config.Leds[i][1];
                            var row = Config.Leds[i][2];
                            var label = Controls.Find($"0-{col}-{row}", false).First();
                            label.BackColor = ledColor;
                            label.ForeColor = Color.FromArgb(255, (byte)(ledColors[i, 0] + 128),
                                (byte)(ledColors[i, 1] + 128), (byte)(ledColors[i, 2] + 128));
                        }
                        //Console.WriteLine(string.Join(", ", colors.Select(color => $"[{color.R},{color.G},{color.B}]")));
                    };
                    Invoke(updateLedUi);
                }
            });
            updateLedUiTask.Start();
        }

        private void StartSerialCommunicationTask()
        {
            var serialCommuncationTask = new Task(() =>
            {
                ISerialWriter serialWriter;
                if (Config.EnableSerialCommunication)
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
            _serialDataAvailable.Set();
            _ledDataAvailable.Set();
            _capture.Enabled = true;
        }

        private void lightsOn_Click(object sender, EventArgs e)
        {
            _lightsOn.Enabled = false;
            var serialWriter = new AdaLedSerialWriter();
            var random = new Random();
            var colors = new byte[Config.Leds.GetLength(0) * 3];
            for (var i = 0; i < Config.Leds.GetLength(0) * 3; i++)
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

        protected override void OnClosing(CancelEventArgs e)
        {
            _cancel?.Cancel(false);
            Application.DoEvents();
            base.OnClosing(e);
        }
    }
}