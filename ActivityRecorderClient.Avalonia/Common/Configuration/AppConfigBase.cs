using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
    public abstract class AppConfigBase
    {
        public static class EndpointIdentity
        {
            public static System.ServiceModel.EndpointIdentity CreateX509CertificateIdentity(X509Certificate2 certificate) =>
                new X509CertificateEndpointIdentity(certificate);
            public static System.ServiceModel.EndpointIdentity CreateUpnIdentity(string upnName) => new UpnEndpointIdentity(upnName);
        }

        public class NetTcpBinding : System.ServiceModel.NetTcpBinding
        {
            public bool TransactionFlow { get; set; }
            public TransactionProtocol TransactionProtocol { get; set; }
            public HostNameComparisonMode HostNameComparisonMode { get; set; }
            public int ListenBacklog { get; set; }
        }

        public enum TransactionProtocol
        {
            Default,
            OleTransactions
        }
    }
}
