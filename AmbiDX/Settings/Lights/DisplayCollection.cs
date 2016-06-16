using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace AmbiDX.Settings.Lights
{
    [ConfigurationCollection(typeof(DisplayElement))]
    public class DisplayCollection : ConfigurationElementCollection, IEnumerable<DisplayElement>
    {
        protected override string ElementName => "display";

        protected override ConfigurationElement CreateNewElement()
        {
            return new DisplayElement();
        }
     
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (DisplayElement)element;
        }

        public new IEnumerator<DisplayElement> GetEnumerator()
        {
            return BaseGetAllKeys().Select(key => (DisplayElement)BaseGet(key)).GetEnumerator();
        }

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
    }
}