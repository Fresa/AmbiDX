using System.Configuration;

namespace AmbiDX.Settings.Process
{
    public class ProcessSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ProcessCollection Processes => (ProcessCollection)base[""];
    }
}