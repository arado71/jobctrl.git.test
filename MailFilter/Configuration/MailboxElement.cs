using System;
using System.ComponentModel;
using System.Configuration;
using log4net;

namespace Tct.MailFilterService.Configuration
{
    public class MailBoxElement : ConfigurationElement
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PeriodicManager));

        private static readonly ConfigurationProperty attrName = new ConfigurationProperty("Name", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty attrAddress = new ConfigurationProperty("Address", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty attrStatus = new ConfigurationProperty("Status", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);

        public MailBoxElement()
        {
            base.Properties.Add(attrName);
            base.Properties.Add(attrAddress);
            base.Properties.Add(attrStatus);
        }
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (string)this[attrName]; }
        }
        [ConfigurationProperty("Address", IsRequired = true)]
        public string Address
        {
            get { return (string)this[attrAddress]; }
        }
        [ConfigurationProperty("Status", IsRequired = true)]
        public IssueState Status
        {
            get
            {
                try
                {
                    return (IssueState) Enum.Parse(typeof(IssueState), this[attrStatus].ToString());
                }
                catch
                {
                    log.Error("Unknown Status value in configuration",
                        new InvalidEnumArgumentException((string) this[attrStatus]));
                    return IssueState.WaitingForCustomer;
                }
            }
        }
    }
}
