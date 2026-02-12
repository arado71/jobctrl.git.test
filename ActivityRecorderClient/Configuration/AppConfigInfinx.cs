using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TctTransport;
using Tct.ActivityRecorderClient.Notification;
using Binding = System.ServiceModel.Channels.Binding;
using TctEncoder;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigInfinx || DEBUG

	public class AppConfigInfinx : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.infinx.com:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.infinx.com/JobCTRL/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.infinx.com:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@jc360.infinx.com")
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://jc360.infinx.com/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@jc360.infinx.com")
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

		#region ServiceTcpBinZip

		protected virtual EndpointAddress ServiceTcpBinZipEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.infinx.com:9000/ActivityRecorderService2", UriKind.Absolute)
		);

		protected virtual Binding ServiceTcpBinZipBinding => new CustomBinding()
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

		protected virtual int? ServiceTcpBinZipMaxItemsInObjectGraph { get; } = 2147483647;

		protected virtual bool ServiceTcpBinZipCertificateValidationDisabled { get; } = false;

		#endregion ServiceTcpBinZip

		protected override void Initialize()
		{
			base.Initialize();
			if (ServiceTcpBinZipEndpointAddress != null)
				ServiceEndpointConfigurations.Add("ServiceTcpBinZip", new EndpointConfiguration("ServiceTcpBinZip", ServiceTcpBinZipEndpointAddress, ServiceTcpBinZipBinding, ServiceTcpBinZipMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ServiceTcpBinZipMaxItemsInObjectGraph.Value) : null, ServiceTcpBinZipCertificateValidationDisabled, 0));
		}

		public override string WebsiteUrl => "https://jc360.infinx.com/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "Infinx";
		public override string TaskPlaceholder => "JC360";
		public override string AppNameOverride => "JC360";
		public override bool IsTaskBarIconShowing => false;
		public override NotificationPosition NotificationPosition => NotificationPosition.Hidden;
		public override Keys? ManualMeetingHotKey => null;
		public override bool SuppressActiveDirectoryFallbackLogin => true;
	}

#endif
}
