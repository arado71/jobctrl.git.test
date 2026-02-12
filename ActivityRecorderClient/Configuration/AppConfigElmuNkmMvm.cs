using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigElmuNkmMvm || DEBUG

	public class AppConfigElmuNkmMvm : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://plu-ps-jcapp-a.pluto.local:9000/ActivityRecorderService", UriKind.Absolute),
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

		protected override EndpointAddress ServiceHttpsEndpointAddress => null; // new EndpointAddress(
																				//	new Uri("https://plu-ps-jcapp-a.pluto.local/JobCTRL/", UriKind.Absolute)
																				//);

		//protected override EndpointAddress ServiceBinZipHttpsEndpointAddress => new EndpointAddress(
		//	new Uri("https://plu-ps-jcapp-a.pluto.local/JC/", UriKind.Absolute)
		//);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://plu-ps-jcapp-a.pluto.local:9000/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@mvmee.hu")
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://plu-ps-jcapp-a.pluto.local/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@mvmee.hu")
		);

		protected override Binding ServiceHttpsBinding => new CustomBinding()
		{
			Name = ServiceHttpsName,
			CloseTimeout = new TimeSpan(0, 1, 0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			Elements =
			{
				SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
				new BinaryMessageEncodingBindingElement()
				{
					ReaderQuotas = new XmlDictionaryReaderQuotas()
					{
						MaxDepth = DefaultMaxDepth,
						MaxStringContentLength = DefaultMaxStringContentLength,
						MaxArrayLength = 100001,
						MaxBytesPerRead = DefaultMaxBytesPerRead,
						MaxNameTableCharCount = DefaultMaxNameTableCharCount,
					}
				},
				new HttpsTransportBindingElement()
				{
					AllowCookies = false,
					BypassProxyOnLocal = false,
					HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
					MaxBufferSize = 5000001,
					MaxBufferPoolSize = 12000001,
					MaxReceivedMessageSize = 5000001,
					TransferMode = TransferMode.Buffered,
					UseDefaultWebProxy = true,
				}
			}
		};

		public override string WebsiteUrl => "https://jc360.mvmee.hu/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "MVM";
		public override bool IsRoamingStorageScopeNeeded => true;
		public override string ValidCertificate => "30818902818100F8DBD15A3CFA5CFB8DF157DA633544461C86F4546AE4B8A0E46B14C62D2F0AFA0E2552B4344A0386665F1D5A672F7402673FAF22311ED10AA699E0D78051D7CBF87D777F5BAAECBD8757DB8112F96308466CAA415135A26B574E95FEE4BB1F41089983DD1B2A8A2AC3A2D5383924DBA919643FE8132E081FAFBC2C3E12AAFB1F0203010001";
		public override IssuePropColumnFlag IssuePropColumns => IssuePropColumnFlag.CategoryVisible;
		public override string IssueCategories => "E+|Földgáz|Több termék|Villamosenergia";
	}

#endif
}
