using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
//using TctEncoder;
//using TctTransport;

namespace Tct.ActivityRecorderClient.Configuration
{
	public abstract class AppConfigLiveAbstract : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => null;

		protected override EndpointAddress ServiceBinZipHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://backoffice.jobctrl.com:443/JC/", UriKind.Absolute));

/*
		#region ServiceTcpBinZip

		protected virtual EndpointAddress ServiceTcpBinZipEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.com:9000/ActivityRecorderService2", UriKind.Absolute)
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
				ServiceEndpointConfigurations.Add(ServiceNetTcpName, new EndpointConfiguration(ServiceNetTcpName, ServiceTcpBinZipEndpointAddress, ServiceTcpBinZipBinding, ServiceTcpBinZipMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ServiceTcpBinZipMaxItemsInObjectGraph.Value) : null, ServiceTcpBinZipCertificateValidationDisabled, 0));
		}
*/
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => null;
	}
}
