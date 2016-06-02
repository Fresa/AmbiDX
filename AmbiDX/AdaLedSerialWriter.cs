using System;
using System.IO.Ports;
using PixelCapturer;

namespace AmbiDX
{
    public class AdaLedSerialWriter : ISerialWriter
    {
        private readonly SerialPort _serialPort;
        private readonly byte[] _header;

        public AdaLedSerialWriter()
        {
            _serialPort = new SerialPort(Config.SerialPort.ToString().ToUpper(), Config.SerialBaud)
            {
                WriteTimeout = 500
            };
            try
            {
                _serialPort.Open();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Port not open. Open ports: " + string.Join(", ", SerialPort.GetPortNames()), ex);
            }
            _header = CreateHeader();
        }

        private static byte[] CreateHeader()
        {
            var serialData = new byte[6];
            // A special header / magic word is expected by the corresponding LED
            // streaming code running on the Arduino.  This only needs to be initialized
            // once (not in draw() loop) because the number of LEDs remains constant:
            serialData[0] = (byte)'A';  // Magic word
            serialData[1] = (byte)'d';
            serialData[2] = (byte)'a';
            serialData[3] = (byte)((Config.Leds.Length - 1) >> 8);   // LED count high byte
            serialData[4] = (byte)((Config.Leds.Length - 1) & 0xff); // LED count low byte
            serialData[5] = (byte)(serialData[3] ^ serialData[4] ^ 0x55); // Checksum

            return serialData;
        }

        public void Write(byte[] bytes)
        {
            var serialData = new byte[_header.Length + bytes.Length];
            _header.CopyTo(serialData, 0);
            bytes.CopyTo(serialData, _header.Length);
            _serialPort.Write(serialData, 0, serialData.Length);
        }

        public void Dispose()
        {
            _serialPort.Close();
        }
    }
}