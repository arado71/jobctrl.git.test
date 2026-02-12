using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigTelekom || DEBUG

	public class AppConfigTelekom : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.telekom.intra:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2(Convert.FromBase64String(@"MIIDFDCCAgCgAwIBAgIQRR9bQH8a37lCmEOCGR48tDAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTE0MDkwNDEzNTE1NVoXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCK0tnN4S06vbVgSCQP9jKk8ZUsG51qXvia0/Pnrm2CtLc7R6Z73U4NtJm4tucatVmpiI+T+G3YlTrVD8pSoFVElHyzXgtcWpRf6aVcDZj3S7lS3t33/KYFq3Lka6+8SqWuldNHTu81fc94iZi0pL5z0nWoDwK6NpVcaDZEYeOYJbnV4T7vVZ8ZjJ2WZ++Q87ZS4GOL4G5OqC8gPWKk+IlnJgdy+/wuiRqSAl/wa+F6fRJVfJkcUGz+02+iVdGdLf70rtLTNJK1O7x2+LEGhgwh9lPVVf12ANDAXwyuoawI+6JXZTPcX8CuMxjY1yjqkPVu+91F8gQKmSJogSewn9rlAgMBAAGjVDBSMFAGA1UdAQRJMEeAEM/+I9gWGCGdMwrLoCRymt6hITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQRR9bQH8a37lCmEOCGR48tDAJBgUrDgMCHQUAA4IBAQBbsjD1TOUAZKc0HMiqqo7Id9p9/KxxIvhnOG4Fib9tFmKo6jIINEKD8x1kMGCoYmWGbQuvYlbS5V0q9hMtrsfmZ58rXy/r+3t39IbHVknpGwdWBmf8ZjH/TN4kL6bsQd2fNzzveYUpp7GKCY0B8bvzysCsjuIbLOuErMCMzmUE+r8q/d7tOAqoOK8jdubpkTsDrJE8vAMXE8pYKspll0EDQbTSprAjtpkCyv8ZEDrXkbvjrSL9wmy3I/vzm50aqHGWYz4A/C8NkpwVN7l6cIKWjbCf+uhW0MfFQa7pSdHjeUkAeg1SRbx5FvNtL6dqKaK5yswdncIh8AlSiRBCAZmo")))
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => null;

		protected override EndpointAddress ActiveDirectoryNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.telekom.intra:9000/ActiveDirectoryLoginService", UriKind.Absolute),
			EndpointIdentity.CreateUpnIdentity(@"_jobctrlapp@res.hu.corp")
		);

		public override string WebsiteUrl => "https://jobctrl.telekom.intra/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "Telekom";
		public override bool IsRoamingStorageScopeNeeded => true;
		public override bool? IsRunAsAdminDefault => false;
	}

#endif
}
