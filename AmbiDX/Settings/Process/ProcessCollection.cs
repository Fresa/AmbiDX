using System.Configuration;

namespace AmbiDX.Settings.Process
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
}