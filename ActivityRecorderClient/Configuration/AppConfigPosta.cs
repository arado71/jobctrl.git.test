using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TctEncoder;
using TctTransport;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigPosta || DEBUG

	public class AppConfigPosta : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.tech01.corp.posta.hu:9000/ActivityRecorderService2", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override Binding ServiceNetTcpBinding => new CustomBinding()
		{
			CloseTimeout = new TimeSpan(0, 1, 0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			Elements =
			{
				SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
				new TctMessageEncodingBindingElement()
				{
					InnerMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement(),
				},
				new TctTransportBindingElement()
				{
					TransferMode = TransferMode.Buffered,
					HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
					ListenBacklog = 10,
					MaxBufferPoolSize = 12000001,
					MaxBufferSize = 5000001,
					MaxPendingConnections = 10,
					MaxPendingAccepts = 10,
					MaxReceivedMessageSize = 5000001,
				}
			}
		};

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.tech01.corp.posta.hu/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.tech01.corp.posta.hu:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.tech01.corp.posta.hu/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@jc360.tst.vftk.posta.hu")
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

		public override string WebsiteUrl => "https://jc360.tech01.corp.posta.hu/";
		public override string ValidCertificate => "3082010A0282010100BBBB5378667CC62D127E3E272215EAE9850C8DD86469996D7719D70445A15CB3735A67E6B95DF453F68EBD87A8EC6EE3DAEAB3A2A82A451311C76CCFC486576FA29A8B6E66E7540283DADBCC42007CD64BAD6101D25342F8636958F20770FC2881D06F1CEF968A1F84647345F7A9641CAE991D8BAD8A4E73D10639AF545CE15E88B643C005564CF8A622E4508B9F5E6D78BF35CABE3BAE926BE563A25C2A906BCBC800B08E45C1A72BDDD288A30CD3010F0495FA46D32326575D6E8C6E1350701333BAF80A65CA6C7F96583DCA952F8B1C502248A49A4B7EBEA7B3A63A60BFE2A31FF4157E12FE46DD8BB977560499C6F4936C51B5B4E258D98D393235C461990203010001";
		public override string GoogleClientId => null;
		public override string GoogleClientSecret => null;
		public override string AppClassifier => "Posta";
		public override string TaskPlaceholder => "JC360";
		public override string AppNameOverride => "JC360";
	}

#endif
}
