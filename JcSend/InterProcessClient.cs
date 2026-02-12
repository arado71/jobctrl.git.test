using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using JcSend.InterProcessReference;

namespace JcSend
{
	public class InterProcessClient : IDisposable
	{
#if DEBUG
		private const string IpcEndPointName = "JcIpc-dbg";
#else
#if DEV
		private const string IpcEndPointName = "JcIpc-dev";
#else
		private const string IpcEndPointName = "JcIpc";
#endif
#endif
		private readonly Binding netNamedPipeBinding = new NetNamedPipeBinding()
		{
			CloseTimeout = new TimeSpan(0,1,0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			TransactionFlow = false, TransferMode = TransferMode.Buffered,TransactionProtocol = TransactionProtocol.OleTransactions,
			HostNameComparisonMode = HostNameComparisonMode.StrongWildcard, MaxBufferPoolSize = 524288,
			MaxBufferSize = 65536, MaxConnections = 10, MaxReceivedMessageSize = 65536,
			ReaderQuotas = new XmlDictionaryReaderQuotas()
			{
				MaxDepth = 32, MaxStringContentLength = 8192, MaxArrayLength = 16384,
				MaxBytesPerRead = 4096, MaxNameTableCharCount = 16384,
			},
			Security = new NetNamedPipeSecurity()
			{
				Mode = NetNamedPipeSecurityMode.Transport,
				Transport = new NamedPipeTransportSecurity() { ProtectionLevel = ProtectionLevel.EncryptAndSign }
			}
		};
		private const string endpointUri = "net.pipe://localhost/" + IpcEndPointName;
		private readonly InterProcessServiceClient client;

		public InterProcessClient()
		{
			client = new InterProcessServiceClient(netNamedPipeBinding, new EndpointAddress(endpointUri));
		}

		public void AddProjectAndWorkByRule(string projectKey, string workName, string workKey, int ruleId)
		{
			client.AddProjectAndWorkByRule(projectKey, workName, workKey, ruleId);
		}

		public void StartWork(int workId)
		{
			client.StartWork(workId);
		}

		public void StopWork()
		{
			client.StopWork();
		}

		public void SwitchWork(int workId)
		{
			client.SwitchWork(workId);
		}

		public void AddExtText(string text)
		{
			client.AddExtText(text);
		}

		public void Dispose()
		{
			if (client == null) return;
			bool cleanedUp = false;
			if (client.State != CommunicationState.Faulted)
			{
				try
				{
					client.Close();
					cleanedUp = true;
				}
				catch
				{
				}
			}
			if (!cleanedUp)
			{
				try
				{
					client.Abort();
				}
				catch //we don't expect exceptions here... but just in case
				{
				}
			}
		}
	}

	public enum InterProcessCommand
	{
		AddProjectAndWorkByRule,
		StartWork,
		StopWork,
		SwitchWork,
		AddExtText,
	}
}
