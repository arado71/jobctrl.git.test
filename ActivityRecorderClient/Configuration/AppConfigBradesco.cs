using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Tct.ActivityRecorderClient.Notification;
using Binding = System.ServiceModel.Channels.Binding;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigBradesco || DEBUG

	public class AppConfigBradesco : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.bradesco.com.br:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.bradesco.com.br/JobCTRL/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.bradesco.com.br:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.bradesco.com.br/ActiveDirectoryLoginService", UriKind.Absolute)
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

		public override string WebsiteUrl => "https://jc360.bradesco.com.br/";
		public override string AppClassifier => "Bradesco";
		public override string ValidCertificate => "3082010A0282010100E9071E4F20625DC0DCBA476F5E3B84F587CBA47B768C3E1A25C3B310654F192D78A1F5220DB182C173FDED71CC64A456A8061C466307FB23C54AFF731293094924F5673378BBE1530265983345651F3750A1A2A820D4D7268B6E0647B2A76796AA62AB83F53639E2E7EBECC2FED79542FF058F669C316D6F2731D79594779956BDD248056DA5E3358EC2D1D8CDB4722A8DE9884B38EC13A58BDE937DC1C7CC6010FAC8E180050227E6077B758F60F6D3F9FE42A0CB6768FA3BDE8DD4DD494005D8FDEF443EE6534DF3BB1500ACF5A1776BBD64958D27C67339DC43C2C85BE000C8A869D7C1471A32BADAF013E4DB795F046F3A18F55A3F5F816AEF9BF91FFBF50203010001";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string TaskPlaceholder => "JC360";
		public override string AppNameOverride => "JC360";
		public override bool IsTaskBarIconShowing => false;
		public override NotificationPosition NotificationPosition => NotificationPosition.Hidden;
		public override Keys? ManualMeetingHotKey => null;
		public override bool SuppressActiveDirectoryFallbackLogin => true;
	}

#endif
}
