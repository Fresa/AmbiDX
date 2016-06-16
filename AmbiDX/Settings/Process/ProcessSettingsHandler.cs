using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace AmbiDX.Settings.Process
{
    public static class ProcessSettingsHandler
    {
        private const string ConfigSection = "processes";

        public static IEnumerable<string> GetProcessNames()
        {
            return Section.Processes.Cast<ProcessElement>().Select(process => process.Name);
        }

        public static void Save(ProcessElement process)
        {
            if (Contains(process))
            { 
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
            var section = config.GetSection(ConfigSection) as ProcessSection;
            if (section == null)
            {
                throw new ConfigurationErrorsException($"Could not find {ConfigSection} configuration in app.config");
            }

            section.Processes.Add(process);

            config.Save();
            ConfigurationManager.RefreshSection(ConfigSection);
        }

        public static void Delete(ProcessElement process)
        {
            if (Contains(process) == false)
            {
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var section = config.GetSection(ConfigSection) as ProcessSection;
            if (section == null)
            {
                throw new ConfigurationErrorsException($"Could not find {ConfigSection} configuration in app.config");
            }

            section.Processes.Remove(process);

            config.Save();
            ConfigurationManager.RefreshSection(ConfigSection);
        }

        private static bool Contains(ProcessElement process)
        {
            return Section.Processes.Cast<ProcessElement>().Any(processElement => processElement.Name == process.Name);
        }

        private static ProcessSection Section
        {
            get
            {
                var section = ConfigurationManager.GetSection(ConfigSection) as ProcessSection;
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