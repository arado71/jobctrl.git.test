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
#if AppConfigUly || DEBUG

	public class AppConfigUly : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jc360.alig.hu:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2(Convert.FromBase64String(@"MIIDFDCCAgCgAwIBAgIQ+ITxZwD/wI1NZnoLX7kOLTAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTEzMDkzMDE0NTA0NFoXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC8y3ZmRmqXLQUXZFTeiD4/1tmG2gNug/cgGIvprdXq3tAt+utJB9We8XusoqRLK9GojYMhBh2sClQFQg6LRVz3DGNBDqryVT18/eoOOdVwAKaS+2oxBy/SNSrkJ/FmM+vbymiDzc/6YeDC4yVHA3mgxhhUebgd7+9cvh5n+86O+NP/JAXZCvh3l7yGlZ0FlVCbqahutwTf7QourncltwLKchejvQIH9cBR0f+5OHxF5XPmNy+xYR0Zgv9hzhQvnf4KbZMMbix3uQBs/AsYy9sqN5JU7jSAwQSHJyo5/RhTeL8skZt18p073EfpWhGSOIBxslnaVr7Wn4gfxnWh2sn1AgMBAAGjVDBSMFAGA1UdAQRJMEeAEF9RfQ4TYJqIhEjX9AEhm2qhITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQ+ITxZwD/wI1NZnoLX7kOLTAJBgUrDgMCHQUAA4IBAQCrJcEd+6ZpvKN7J11hyWfqdaMdS6KW46a27ElLxqu5Dfm8ZX1pids/osRbOML/1rtAOSFTerG1TEkqhHYvfvEgrO6ip4p9T3PJBrQ3bqSFGKBPCFcMS1+6mO4JAIS2IKD8uEkHw1jotX11lqJFN8I1ZeBrrQwxYVjaXMXOgIMhRimeWOEoyPEJZL5340RndXEkz1SMN8GAeIQ9qVL0ozMtP8+ZCHFBOLvCSr6xokFxJcycFvV6wawPMVXWQYnw6H67DkwsNPnkUs6iIR9mUdPlcEaKJRO737Zy3zEZ3wfB8LTIJNKSF7mAo2aNoaJoQ/y29W/IPBEdmdSv2rC/uDnf")))
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => null;

		public override string WebsiteUrl => "https://jc360.alig.hu/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "Ulyssys";
	}

#endif
}
