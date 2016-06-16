using System.Configuration;

namespace AmbiDX.Settings.Preview
{
    public static class PreviewConfig
    {
        private const string ConfigSection = "preview";

        public static PreviewSection Get()
        {
            return Section;
        }

        private static PreviewSection Section
        {
            get
            {
                var section = ConfigurationManager.GetSection(ConfigSection) as PreviewSection;
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