using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigOfsz || DEBUG

	public class AppConfigOfsz : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.ofsz.hu:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.ofsz.hu/JobCTRL/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.ofsz.hu:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://jc360.ofsz.hu/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@jc360.ofsz.hu")
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

		public override string WebsiteUrl => "https://jc360.ofsz.hu/";
		public override string GoogleClientId => null;
		public override string GoogleClientSecret => null;
		public override string AppClassifier => "(OFSZ)";
		public override string CalendarFolderInclusionPattern => @"^([\\][\\]([^@\\]+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17}))|(Postaláda [^\\]*)|(Postafiók [^\\]*))[\\](Naptár|Calendar)$";
	}

#endif
}
