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
#if AppConfigOTP || DEBUG

	public class AppConfigOTP : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.otpbank.hu:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://bo-jc360.otpbank.hu/JobCTRL/", UriKind.Absolute)
		);

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.otpbank.hu:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ActiveDirectoryHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://jc360.otpbank.hu/ActiveDirectoryLoginService", UriKind.Absolute)
			, EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@jc360.otpbank.hu")
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

		public override string WebsiteUrl => "https://jc360.otpbank.hu/";
		public override string GoogleClientId => null;
		public override string GoogleClientSecret => null;
		public override string AppClassifier => "OTP";
		public override string ValidCertificate => "3082010A0282010100B71F875E436A3BA16B4BFEA30119D08ADF93BD995635AA0D06DADAE2A554DE28992C49A013F792CD6ACD693D9656409072BE6DB343EE7BA1CF09D17336E53D23D95C56B1772C10C57FFA78ED78B9581004A63C003BAA6B08B816B0114ABFF82659FB2E764C51869A2D56B4FCBE72DF997E5184582262D8FC05622C90D59C63F8A10EBCECF8FB6B7310245DF0F2192C0BCE1D1D8F1CD3317D0375D17E3B3A5167E8D2E98691A8C5102E508D31ED41C7F10A11F0DFEE1A2092D7A681558146023D3BF9D9FD2B483800EBC7A1FBCDF3AA80E89A1E8B085CB22CABDBD52D53FF17B9B2AB6E768052B23AFCA6436DE9E18CD7006C2B42034DBD068005C14E50765CD90203010001";
		public override string TaskPlaceholder => "JC360";
		public override string AppNameOverride => "JC360";
	}

#endif
}
