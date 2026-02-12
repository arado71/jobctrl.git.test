using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigElmu || DEBUG

	public class AppConfigElmu : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => null;

		protected override EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://VAWWFMAPP:9002/JobCTRL/", UriKind.Absolute)
		);

		public override string WebsiteUrl => "http://jobctrl.elmu.hu/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "Elmu";
		public override bool IsRoamingStorageScopeNeeded => true;
		public override string ValidCertificate => "30818902818100F8DBD15A3CFA5CFB8DF157DA633544461C86F4546AE4B8A0E46B14C62D2F0AFA0E2552B4344A0386665F1D5A672F7402673FAF22311ED10AA699E0D78051D7CBF87D777F5BAAECBD8757DB8112F96308466CAA415135A26B574E95FEE4BB1F41089983DD1B2A8A2AC3A2D5383924DBA919643FE8132E081FAFBC2C3E12AAFB1F0203010001";
		public override IssuePropColumnFlag IssuePropColumns => IssuePropColumnFlag.CategoryVisible;
		public override string IssueCategories => "E+|Földgáz|Több termék|Villamosenergia";
	}

#endif
}
