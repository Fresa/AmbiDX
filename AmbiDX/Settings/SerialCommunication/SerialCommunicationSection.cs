using System;
using System.Configuration;
using PixelCapturer;

namespace AmbiDX.Settings.SerialCommunication
{
    public class SerialCommunicationSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled => (bool)base["enabled"];

        [ConfigurationProperty("baud", IsRequired = true)]
        public int Baud => (int)base["baud"];

        [ConfigurationProperty("port", IsRequired = true)]
        public Port Port => (Port)Enum.Parse(typeof(Port), base["port"].ToString());
    }
}