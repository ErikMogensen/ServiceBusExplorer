using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExplorer.Helpers
{
    public class NamespaceConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("Name")]
        public string Name
        {
            get => base["Name"] as string;
            set => base["Name"] = value;
        }

        [ConfigurationProperty("ConnectionString")]
        public string ConnectionString
        {
            get => base["ConnectionString"] as string;
            set => base["ConnectionString"] = value;
        }

        [ConfigurationProperty("Entity")]
        public string Entity
        {
            get => base["Entity"] as string;
            set => base["Entity"] = value;
        }
    }


    [ConfigurationCollection(typeof(NamespaceConfigurationElement), 
        AddItemName = "Namespace", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class NamespaceConfigurationElementCollection : ConfigurationElementCollection
    {
        public NamespaceConfigurationElement this[int index]
        {
            get => (NamespaceConfigurationElement)BaseGet(index);
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        public void Add(NamespaceConfigurationElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NamespaceConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NamespaceConfigurationElement)element).Name;
        }

        public void Remove(NamespaceConfigurationElement serviceConfig)
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
