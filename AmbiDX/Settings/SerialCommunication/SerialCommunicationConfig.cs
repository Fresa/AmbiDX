using System.Configuration;

namespace AmbiDX.Settings.SerialCommunication
{
    public static class SerialCommunicationConfig
    {
        private const string ConfigSection = "serialCommunication";

        public static SerialCommunicationSection Get()
        {
            return Section;
        }

        private static SerialCommunicationSection Section
        {
            get
            {
                var section = ConfigurationManager.GetSection(ConfigSection) as SerialCommunicationSection;
                if (section == null)
                {
                    throw new ConfigurationErrorsException(
                        $"Could not find {ConfigSection} configuration in app.config");
                }
                return section;
            }
        }
    }
}