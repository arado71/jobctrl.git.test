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
#if AppConfigIbm || DEBUG

	public class AppConfigIbm : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://ibmcgsrvjc:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2(Convert.FromBase64String(@"MIIDFDCCAgCgAwIBAgIQSwPiN6xv96NNqbzhszGywzAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTE0MDYzMDEwNDQyMFoXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDCgeD6kdVqrSqwWzLQZEPCOTUph7UwygZ13Fxp/a15rI/pu1HdmYpdEhxNvZcbpHX9D/FgGtV8BREyoi2YxYv52Kbw1wsCoa4W7MwD6GLNygcXlXQpHdPszcK7q+A7z7YHIRE461mda+K8ZQBRA7i/+vExzLa/lgwxkEXIOfGovCDIb3idup7TrtyHCJVCLMHfjNj4WS1m7xbdklNmSmv2aDBhnsT4++j6/9D/2nFbeFeFGnFjFJvgLbae5LTAQy25QVpowASm1goiPZWKwgmmKYhtuRboaglPzR87mf0lbT3ASQWnMBowN9vekrG82/6LDU2kmhezLuEvh4Wdj0djAgMBAAGjVDBSMFAGA1UdAQRJMEeAEJpFHTaDPViqkA3qDvbfGvOhITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQSwPiN6xv96NNqbzhszGywzAJBgUrDgMCHQUAA4IBAQA9iC8IBgcwyRwx5Gh7ppmGvMxJVBMx1s8H0ZEEv/J40iojffS1oN9uC8i0ge7prUv/b8yIOJ6OuSaP6E8cs5U/rG/1kMX6brNyqydcI1xsGHmPFMmBH3oPkWcMq2plzyBIp26/D2x6+Vuz3ey6nM8l56U7yhOcMrfAGgDweuaCz6wwsY2yih93Ufk3D7K/gYwP4afo/dVIJcSXeS7WwAaYEng6OLap7o6ALhgrt4JKBexT2mIayWZ9N6DcRpPB7FRejLmIG0Gjj60jRNQTTbv2kRARgbKwobJwdcSdhXqD5EgssyTVt2QVfkrfAyOzM/2Pr59mTs9kQJWd5ikLYpfk")))
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => null;

		public override string WebsiteUrl => "http://ibmcgsrvjc/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "Ibm";
	}

#endif
}
