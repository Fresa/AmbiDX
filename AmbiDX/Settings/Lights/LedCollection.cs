using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace AmbiDX.Settings.Lights
{
    [ConfigurationCollection(typeof(LedElement))]
    public class LedCollection : ConfigurationElementCollection, IEnumerable<LedElement>
    {
        protected override string ElementName => "led";
        
        protected override ConfigurationElement CreateNewElement()
        {
            return new LedElement();
        }
        
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (LedElement)element;
        }

        public new IEnumerator<LedElement> GetEnumerator()
        {
            return BaseGetAllKeys().Select(key => (LedElement)BaseGet(key)).GetEnumerator();
        }

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
    }
}