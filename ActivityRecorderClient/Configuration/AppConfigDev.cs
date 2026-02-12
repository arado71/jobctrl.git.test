using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using TctEncoder;
using TctTransport;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigDev || DEBUG

	public class AppConfigDev : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://dev.jobctrl.net:9000/ActivityRecorderService2", UriKind.Absolute)
			,EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
			//,EndpointIdentity.CreateDnsIdentity("tct.ActivityRecorder")
		);

		protected override Binding ServiceNetTcpBinding => new CustomBinding
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
			new Uri("https://dev.jobctrl.net:443/JobCTRL/", UriKind.Absolute)
		);

		protected override bool ServiceHttpsCertificateValidationDisabled => true;

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://dev.jobctrl.net:9020/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateUpnIdentity(@"JCAPP1\ccenter")
			//EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://dev.jobctrl.net:9002/ActiveDirectoryLoginService", UriKind.Absolute)
			,EndpointIdentity.CreateUpnIdentity(@"JCAPP1\ccenter")
		);

		public override string WebsiteUrl { get; } = "http://dev.jobctrl.net/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier { get; } = "Dev";
		public override string MutexName => "ActivityRecorderClientDEV";
	}

#endif
}
