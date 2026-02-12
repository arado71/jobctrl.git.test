using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigJapanDeloitte || DEBUG

	public class AppConfigJapanDeloitte : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobconductor.jp:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://backoffice.jobconductor.jp/JobCTRL/", UriKind.Absolute)
		);

		protected override EndpointAddress ServiceBinZipHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://backoffice.jobconductor.jp:443/JC/", UriKind.Absolute)
		);

		protected override bool ServiceNetTcpCertificateValidationDisabled => true;

		public override string WebsiteUrl => "https://jobconductor.jp/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "JapanDeloitte";
		public override bool? IsRunAsAdminDefault => false;
	}

#endif
}
