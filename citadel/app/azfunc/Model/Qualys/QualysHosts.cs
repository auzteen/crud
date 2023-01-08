using System.Collections.Generic;
using System.Xml.Serialization;

namespace Citadel.Model.Qualys
{
    public class QualysData
    {
        [XmlElement(ElementName = "RESPONSE")]
        public Hosts response { get; set; }
    }
    public class Hosts
    {
        [XmlElement(ElementName = "DATETIME")]
        public string dateTime { get; set; }

        [XmlElement(ElementName = "HOST_LIST")]
        public HostList hostList { get; set; }
    }

    public class HostList
    {
        [XmlElement(ElementName = "HOST")]
        public List<Host> host { get; set; }
    }

    public class Host
    {
        [XmlElement(ElementName = "ID")]
        public string id { get; set; }

        [XmlElement(ElementName = "IP")]
        public string ip { get; set; }

        [XmlElement(ElementName = "TRACKING_METHOD")]
        public string trackingMethod { get; set; }

        [XmlElement(ElementName = "NETBIOS")]
        public string netbios { get; set; }

        [XmlElement(ElementName = "OS")]
        public string os { get; set; }

        [XmlElement(ElementName = "DNS")]
        public string dns { get; set; }

        [XmlElement(ElementName = "DNS_DATA")]
        public DnsData dnsData { get; set; }
    }

    public class DnsData
    {
        [XmlElement(ElementName = "HOSTNAME")]
        public string hostname { get; set; }

        [XmlElement(ElementName = "DOMAIN")]
        public string domain { get; set; }

        [XmlElement(ElementName = "FQDN")]
        public string fqdn { get; set; }
    }
}