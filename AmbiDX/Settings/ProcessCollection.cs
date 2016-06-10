using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace AmbiDX.Settings
{
    [ConfigurationCollection(typeof(ProcessElement), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    public class ProcessCollection : ConfigurationElementCollection
    {
        internal const string ItemPropertyName = "process";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => ItemPropertyName;

        protected override bool IsElementName(string elementName)
        {
            return elementName == ItemPropertyName;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProcessElement();
        }

        public void Add(ProcessElement element)
        {
            BaseAdd(element);
        }

        public void Remove(ProcessElement element)
        {
            BaseRemove(element.Name);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProcessElement)element).Name;
        }
        public override bool IsReadOnly()
        {
            return false;
        }
    }

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