using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace Tct.ActivityRecorderClient.Configuration
{
#if DEBUG

	public class AppConfigLocal : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://localhost:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override Binding ServiceNetTcpBinding => new NetTcpBinding()
		{
			Name = ServiceNetTcpName,
			CloseTimeout = new TimeSpan(0, 1, 0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			TransactionFlow = false,
			TransferMode = TransferMode.Buffered,
			TransactionProtocol = TransactionProtocol.OleTransactions,
			HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
			ListenBacklog = 10,
			MaxConnections = 10,
			MaxBufferSize = 5000001,
			MaxBufferPoolSize = 12000001,
			MaxReceivedMessageSize = 5000001,
			ReaderQuotas = new XmlDictionaryReaderQuotas()
			{
				MaxDepth = DefaultMaxDepth,
				MaxStringContentLength = DefaultMaxStringContentLength,
				MaxArrayLength = DefaultMaxArrayLength,
				MaxBytesPerRead = DefaultMaxBytesPerRead,
				MaxNameTableCharCount = DefaultMaxNameTableCharCount,
			},
			ReliableSession = new OptionalReliableSession()
			{
				Ordered = true,
				InactivityTimeout = new TimeSpan(0, 10, 0),
				Enabled = false,
			},
			Security = new NetTcpSecurity() { Mode = SecurityMode.TransportWithMessageCredential, Message = new MessageSecurityOverTcp() { ClientCredentialType = MessageCredentialType.UserName } }
		};

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://127.0.0.1:9002/JobCTRL/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://localhost:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://127.0.0.1:9002/ActiveDirectoryLoginService", UriKind.Absolute)
		);

		public override string GoogleClientId => null; // "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => null; // "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => null;
	}

#endif
}
