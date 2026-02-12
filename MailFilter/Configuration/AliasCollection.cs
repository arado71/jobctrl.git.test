using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Tct.MailFilterService.Configuration
{
    [ConfigurationCollection(typeof(MailBoxElement), AddItemName = "add", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class AliasCollection : ConfigurationElementCollection, IEnumerable<MailBoxElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MailBoxElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailBoxElement)element).Name;
        }
        new public MailBoxElement this[string name]
        {
            get { return (MailBoxElement)BaseGet(name); }
        }
        public new IEnumerator<MailBoxElement> GetEnumerator()
        {
            return this.BaseGetAllKeys().Select(key => (MailBoxElement)BaseGet(key)).GetEnumerator();
        }
    }
}
