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
#if AppConfigBme || DEBUG

	public class AppConfigBme : AppConfig
	{
		protected override EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.piholding.hu:9000/ActivityRecorderService", UriKind.Absolute),
			EndpointIdentity.CreateX509CertificateIdentity(new X509Certificate2(Convert.FromBase64String(@"MIIDFDCCAgCgAwIBAgIQA4L7tYL09qJMp5pGXXrORzAJBgUrDgMCHQUAMB8xHTAbBgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTE0MDUwODExMTMxMloXDTM5MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC/49D9aflwcyQ31pktk+BiZKhyz/4JH7nRDuDKwaandbfTXbMh4oeRXJNISfbCAnPraTiXBdeJPZiEOEgfwdJ+UR5Wkal7ooHBKOJt4u88mSUyl3W0xVDkaJ53UtMClY/UF279we1RMViJpUwS5odDA/DY08EGYuIdvLqnK/0Tzn4vtOolmS/WvVCuawcaeRLgfeGFQH4GU0u9w1tbPzvo7ho7TV2ebw+06L2uB5//ankIQL4nOrjPcOmmddksvDMc5N+WhpMvnybmCGlpcsMiVFtc0yIyuOI/5JSq0U0jMTlR5byFvmQGNldrFE6KLEH2dHKcy5tkULJhcHCZOgwrAgMBAAGjVDBSMFAGA1UdAQRJMEeAEG2733HJfhUpJbdCO7IFj++hITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQA4L7tYL09qJMp5pGXXrORzAJBgUrDgMCHQUAA4IBAQAz8zogrnVJfdOaq4KqrwwJDO7rLNpKY6EZN/5mpc5CEZ/B8hF65xEDnb1YxWmXAeBeq0wk3c2/2byy481Oej5N78c0cA+Wy3hCEc8rmFcVlqjppZqtXrphundTdLsWxyXPhs4APJ9g0J74pdTw0ewwTKwPMb4bxRaEBqt8ShHIfnJlyM80h3okPqa+diFhEjx2ONPfN8nZsycYW2RXAW8MC0WJuARRSHl+0hzLGCvys5pCml1y8a5mS6YrNK2YY1hCk7pmlGbsRlwYuBujXfhGgdYHH9P1D5gPHxVuyfdu6L2m+hsrzPNdzOxJeEu44+NPWedjhHom4utacVT1RF2T")))
		);

		protected override EndpointAddress ServiceHttpsEndpointAddress => null;

		public override string WebsiteUrl => "http://jobctrl.piholding.hu/";
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => "BME";

	}

#endif
}
