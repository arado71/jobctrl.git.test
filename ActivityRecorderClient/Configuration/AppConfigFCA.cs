using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigFCA || DEBUG

	public class AppConfigFCA : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.fcaservices.com.br:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2(Convert.FromBase64String(@"MIIDFDCCAgCgAwIBAgIQiMBL2aT53qRE+8Vf2WjVojAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTE2MTAxMTExNDMyOVoXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDETvs4gN88tB2WZTRQWsU9iqwBFauPYrzsCRXqRW3LBYWLIj6gzUoEcyz1MRVGt7EVSPxQltBZ8PIQsSRXdn50u8rRUPf/R/ZvwjBp5/H/eOeLy2NAAPid3TPdwCqKO3+TrsEcrFTYLLR/KnPzjBiuR+ZM/kuIPQU3Ccalgu67ik19ERtcYYDQVED+euBIbnnIL1S4v70pRm05zdfY4GUAJp4OMxnShkx2Kv07SnTp+M4vRXHOeTty6URNPZ35bziDJpfLB2DTVbswiKYcXDsxtd3LVoAYde1pn3a+PAvVv3FYHEVUbZeTMYwAZpClzPSezYlyiW1X6VYWi3lS8KUvAgMBAAGjVDBSMFAGA1UdAQRJMEeAELvxINCZwheGWsLeN27dIWihITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQiMBL2aT53qRE+8Vf2WjVojAJBgUrDgMCHQUAA4IBAQBP1lewTf0OH1Ds/k1wzO+RjzQcdWKfUwFwZWczBDBxP+px0lsFWGmWh1kv2wA4KXgccJz0HmzEbnH3wLDFqd0IjrNbjNsCfKKbXgO/y0DEgJk2WqXns6rBLPArdonAWBFJQT/+kg6pD0UczMhV3ykPT3L8ZNUTGbTZfSv7+CsVvoKmMlt7PAsZiVMa85BRmJk1PyYtBmgQkDInkEw+NVVHt7wR38cgtY4tGfMj9m+ffz369uvjMExzIoHqMhv9eVDPjeTJhpNL4uEAUjNyOOTSy+iZUerKPq39IAy+FyzmprB08BOAkkk71gF41a1UMHx22/n1wIvHK9HzW45pQx9h")))
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => null;

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.fcaservices.com.br:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateUpnIdentity(@"GESCOEUROPE\_JobCtrlApp")
		);

		public override string WebsiteUrl => "http://jobctrl.fcaservices.com.br";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "FCA";
		public override NotificationPosition NotificationPosition => NotificationPosition.Hidden;
		public override bool OnlyDesktopTasksInWorktimeMod => true;
	}

#endif
}
