using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
	public class EndpointConfiguration
	{
		public string Name { get; }
		public int Order { get; }
		private EndpointAddress EndpointAddress { get; }
		private Binding Binding { get; }
		private IEndpointBehavior Behavior { get; }
		private bool IsCertificateValidationDisabled { get; }

		public EndpointConfiguration(string name, EndpointAddress endpointAddress, Binding binding, IEndpointBehavior behavior, bool isCertificateValidationDisabled, int order)
		{
			Name = name;
			EndpointAddress = endpointAddress;
			Binding = binding;
			Behavior = behavior;
			IsCertificateValidationDisabled = isCertificateValidationDisabled;
			Order = order;
		}

		public T CreateClient<T, U>(Func<Binding, EndpointAddress, T> creator) where T : ClientBase<U> where U: class
		{
			var client = creator(Binding, EndpointAddress);
			client.Endpoint.Behaviors.Add(Behavior);
			if (IsCertificateValidationDisabled && client.ClientCredentials != null)
				client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
			return client;
		}

		public string CheckConnection()
		{
			if (EndpointAddress.Uri.Scheme == "https")
			{
				try
				{
					using (var wc = new WebClient())
					{
						wc.DownloadString(EndpointAddress.Uri);
						return $"Http get OK ({EndpointAddress.Uri})";
					}
				}
				catch (WebException ex)
				{
					if (((System.Net.HttpWebResponse)ex.Response)?.StatusCode == HttpStatusCode.Unauthorized) return $"Http get OK (unauthorized,{EndpointAddress?.Uri})";
				}
			}

			var host = EndpointAddress.Uri.Host;
			var port = EndpointAddress.Uri.Port;
			using (var tcpClient = new TcpClient())
			{
				tcpClient.Connect(host, port);
				return $"Tcp port is open ({host}:{port})";
			}
		}

		public override string ToString()
		{
			return $"{Name}: [{EndpointAddress?.Uri.AbsoluteUri}, {Binding?.Name}]";
		}
	}

}
