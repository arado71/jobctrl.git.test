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
#if AppConfigPostaTest || DEBUG

	public class AppConfigPostaTest : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.tst.vftk.posta.hu:9000/ActivityRecorderService2", UriKind.Absolute),
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
			new Uri("http://jc360.tst.vftk.posta.hu/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.tst.vftk.posta.hu:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("http://jc360.tst.vftk.posta.hu/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@jc360.tst.vftk.posta.hu")
		);

		protected override Binding ServiceHttpsBinding => new CustomBinding() // downgraded to http
		{
			Name = ServiceHttpsName,
			CloseTimeout = new TimeSpan(0, 1, 0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			Elements =
			{
				//SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
				new TextMessageEncodingBindingElement()
				{
					ReaderQuotas = new XmlDictionaryReaderQuotas()
					{
						MaxDepth = DefaultMaxDepth,
						MaxStringContentLength = DefaultMaxStringContentLength,
						MaxArrayLength = 100001,
						MaxBytesPerRead = DefaultMaxBytesPerRead,
						MaxNameTableCharCount = DefaultMaxNameTableCharCount,
					},
					MessageVersion = MessageVersion.Soap11
				},
				new HttpTransportBindingElement()
				{
					AuthenticationScheme = AuthenticationSchemes.Basic, // added for http security
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

		protected override Binding ActiveDirectoryHttpsBinding => new WSHttpBinding()
		{
			Name = ActiveDirectoryHttpsName,
			Security = new WSHttpSecurity() { Mode = SecurityMode.Message, Message = new NonDualMessageSecurityOverHttp() { ClientCredentialType = MessageCredentialType.Windows } }
		};

		public override string WebsiteUrl => "http://jc360.tst.vftk.posta.hu/";
		public override string GoogleClientId => null;
		public override string GoogleClientSecret => null;
		public override string AppClassifier => "Posta Teszt";
		public override string TaskPlaceholder => "JC360";
		public override string AppNameOverride => "JC360";
	}

#endif
}
