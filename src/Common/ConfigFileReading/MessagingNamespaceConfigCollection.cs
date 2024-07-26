using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExplorer.Common.ConfigFileReading
{
    [ConfigurationCollection(typeof(MessagingNamespaceConfigElement), AddItemName = "Lane", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class MessagingNamespaceConfigCollection : ConfigurationElementCollection
    {
        public MessagingNamespaceConfigElement this[int index]
        {
            get { return (MessagingNamespaceConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(MessagingNamespaceConfigElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MessagingNamespaceConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MessagingNamespaceConfigElement)element).Name;
        }

        public void Remove(MessagingNamespaceConfigElement serviceConfig)
        {
            BaseRemove(serviceConfig.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(String name)
        {
            BaseRemove(name);
        }

    }
}
