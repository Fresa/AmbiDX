using System;
using System.IO.Ports;
using AmbiDX.Settings.Lights;
using AmbiDX.Settings.SerialCommunication;

namespace AmbiDX
{
    public class AdaLedSerialWriter : ISerialWriter
    {
        private readonly SerialPort _serialPort;
        private readonly byte[] _header;

        public AdaLedSerialWriter()
        {
            _serialPort = new SerialPort(SerialCommunicationConfig.Get().Port.ToString().ToUpper(), SerialCommunicationConfig.Get().Baud)
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
            serialData[0] = (byte)'A'; 
            serialData[1] = (byte)'d';
            serialData[2] = (byte)'a';
            serialData[3] = (byte)((LightsConfig.LedCount - 1) >> 8);   // LED count high byte
            serialData[4] = (byte)((LightsConfig.LedCount - 1) & 0xff); // LED count low byte
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