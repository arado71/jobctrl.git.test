using System;
using System.ComponentModel;
using System.Configuration;
using log4net;

namespace Tct.MailFilterService.Configuration
{
    public class MailFilterConfig : ConfigurationSection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PeriodicManager));

        private static readonly ConfigurationProperty srvAttr = new ConfigurationProperty("Server", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty logAttr = new ConfigurationProperty("Login", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty pasAttr = new ConfigurationProperty("Password", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty prtAttr = new ConfigurationProperty("Port", typeof(int), 443, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty aliAttr = new ConfigurationProperty("Aliases", typeof(AliasCollection), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty delAttr = new ConfigurationProperty("DeleteProcessedMails", typeof(AliasCollection), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty comAttr = new ConfigurationProperty("CompanyId", typeof(AliasCollection), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty authcodeAttr = new ConfigurationProperty("AuthCode", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty sslAttr = new ConfigurationProperty("SslEnabled", typeof(bool), true, ConfigurationPropertyOptions.IsRequired);

        public MailFilterConfig()
        {
            base.Properties.Add(srvAttr);
            base.Properties.Add(prtAttr);
            base.Properties.Add(logAttr);
            base.Properties.Add(pasAttr);
            base.Properties.Add(aliAttr);
            base.Properties.Add(delAttr);
            base.Properties.Add(comAttr);
            base.Properties.Add(authcodeAttr);
            base.Properties.Add(sslAttr);
        }
        [ConfigurationProperty("Server", IsRequired = true)]
        public string Server
        {
            get { return (string)this[srvAttr]; }
        }
        [ConfigurationProperty("Login", IsRequired = true)]
        public string Login
        {
            get { return (string)this[logAttr]; }
        }
        [ConfigurationProperty("Password", IsRequired = true)]
        public string Password
        {
            get { return (string)this[pasAttr]; }
        }
        [ConfigurationProperty("Port", IsRequired = true)]
        public int Port
        {
            get { return (int)this[prtAttr]; }
        }
        [ConfigurationProperty("CompanyId", IsRequired = true)]
        public int CompanyId
        {
            get { return (int)this[comAttr]; }
        }
        [ConfigurationProperty("DeleteProcessedMails", IsRequired = false)]
        public bool DeleteProcessedMails
        {
            get
            {
                try
                {
                    return (bool)Boolean.Parse(this[delAttr].ToString());
                }
                catch
                {
                    log.Error("Unknown DeleteProcessedMails value in configuration",
                        new InvalidEnumArgumentException((string)this[delAttr]));
                    return false;
                }
            }
        }
        [ConfigurationProperty("Aliases", IsRequired = true)]
        public AliasCollection Aliases
        {
            get { return (AliasCollection)this[aliAttr]; }
        }
        [ConfigurationProperty("AuthCode", IsRequired = false)]
        public string AuthCode
        {
            get
            {
                return (string)this[authcodeAttr];
            }
        }
        [ConfigurationProperty("SslEnabled", IsRequired = false)]
        public bool SslEnabled
        {
            get
            {
                return (bool)this[sslAttr];
            }
        }
    }
}
