using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusExplorer.Common.ConfigFileReading
{
    public class MessagingNamespaceConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("Name")]
        public string Name
        {
            get
            {
                return base["Name"] as string;
            }
        }

        [ConfigurationProperty("PointId")]
        public string PointId
        {
            get
            {
                return base["PointId"] as string;
            }
        }

        [ConfigurationProperty("Direction")]
        public Direction? Direction
        {
            get
            {
                return base["Direction"] as Direction?;
            }
        }
    }

    public enum Direction
    {
        Entry,
        Exit
    }

    public class MessagingNamespacesConfigSection : System.Configuration.ConfigurationSection
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(MessagingNamespacesConfigSection));
        public const string SectionName = "ServiceBusNamespaces";

        //[ConfigurationProperty("Credentials")]
        //public MessagingNamespaceConfigElement Credentials
        //{
        //    get
        //    {
        //        return base["Credentials"] as MessagingNamespaceConfigElement;
        //    }
        //}

        //[ConfigurationProperty("PrimaryAgent")]
        //public ServerInfoConfigElement PrimaryAgent
        //{
        //    get
        //    {
        //        return base["PrimaryAgent"] as ServerInfoConfigElement;
        //    }
        //}

        //[ConfigurationProperty("SecondaryAgent")]
        //public ServerInfoConfigElement SecondaryAgent
        //{
        //    get
        //    {
        //        return base["SecondaryAgent"] as ServerInfoConfigElement;
        //    }
        //}

        //[ConfigurationProperty("Site")]
        //public SiteConfigElement Site
        //{
        //    get
        //    {
        //        return base["Site"] as SiteConfigElement;
        //    }
        //}

        [ConfigurationProperty("ServiceBusNamespaces")]
        public MessagingNamespaceConfigCollection MessagingNamespaces
        {
            get { return base["ServiceBusNamespaces"] as MessagingNamespaceConfigCollection; }
        }
    }
}


