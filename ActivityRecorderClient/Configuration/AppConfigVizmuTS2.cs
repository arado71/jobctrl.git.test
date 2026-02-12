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
#if AppConfigVizmuTS2 || DEBUG

	public class AppConfigVizmuTS2 : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.vizmuvek.hu:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2(Convert.FromBase64String(@"MIIDFDCCAgCgAwIBAgIQ3lvkJUivO5BIyTggd4uG+jAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTE2MDEwNjEyNTk1N1oXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC/DTNvF2RJIB++BspjajnFz2LcVb7MAEV+wANfWyMrlTwQbUqL/NNYg4rNrvnkRM4xH9WbYoDvqSD6LeJHckf/jrZ+L+VFdBLNKsfptbHN9bg+TU7h3parJVmCoQGiRGX+DYcvTP+rVSDpWdEhRvQmMykOHrkB5pbNj/9UfogS+xoGL56XDax6Y+rqBfu4lTgFQEORgDZkYEkF16oR8EK0ed/aXdPXetve4bJVmpThlp2DWx4YbM1bYbnF6Gvn8FBZ6Me7f0K3NISbOulj6jUQYsih//XllUDb/BUB7FTX/aOGxaF21p6DEf4eLSSsE+dBK7b2278jlbVxJdk4x7+VAgMBAAGjVDBSMFAGA1UdAQRJMEeAEEh2tx/k7yU9WhCUkEDRNSGhITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQ3lvkJUivO5BIyTggd4uG+jAJBgUrDgMCHQUAA4IBAQCIBWcPkMMdsQ30FlyMw3vHvwVBE6DTe+RZSYoNZAkTq7PBOS/JFoD0D+r34eq3jRCPrufjrk4Dh07mBwAO8z85+RBWP3kCczgBksDwyPG6tij/wTvzCso4xLH9clp14bKmvEavZb13tNkgFqQ3ODAm7SuskZsBbo9BK6lWzej1aSVOaRa5bjtZFKMZiOQT/U4q2UFNrP0p5/d63bEzREKqW1L3vbBuTn0HgWPGPZFoGx5PNgnvUaIr5xy+7YVyWuloKDA1F4JCqd0NKjoc9oxs/AnS7Jk7UiH0o2rPW2ducMiAPQex7eHWiuLHKbra8AjnGD+L0UCczkNgNAtW2bUa")))
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => null;

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.vizmuvek.hu:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateUpnIdentity(@"_JobCtrlApp@vizmuvek.hu")
		);

		public override string WebsiteUrl => "https://jobctrl.vizmuvek.hu/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "FVMTS2";
		public override bool IsRoamingStorageScopeNeeded => true;
		public override bool IsMeetingAppointmentSynchronized => true;
		public override bool IsMeetingDescriptionSynchronized => false;
		public override bool IsMeetingUploadModifications => true;
		public override string TaskPlaceholder => "Timesheet";
		public override string AppNameOverride => "Timesheet2";
	}

#endif
}
