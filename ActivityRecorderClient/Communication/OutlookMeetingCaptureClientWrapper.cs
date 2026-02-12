using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Tct.ActivityRecorderClient.OutlookMeetingCaptureServiceReference;
using log4net;
using System.ServiceModel.Description;
using Tct.ActivityRecorderClient.Configuration;

namespace Tct.ActivityRecorderClient.Communication
{
	public class OutlookMeetingCaptureClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly string outlookSyncServiceEndpointScheme = "net.pipe://localhost/OutlookMeetingCaptureService_{0}";
		private static int parentPID;
		private static string outlookSyncServiceEndpoint;

		public readonly MeetingCaptureServiceClient Client;

		static OutlookMeetingCaptureClientWrapper()
		{
			try
			{
				using (Process process = Process.GetCurrentProcess())
				{
					parentPID = process.Id;
				}

				outlookSyncServiceEndpoint = String.Format(outlookSyncServiceEndpointScheme, parentPID);
			}
			catch (Exception)
			{
				log.Error("Unable to create dynamic enpoint address with parent PID.");
			}
		}

		public OutlookMeetingCaptureClientWrapper()
			: this(outlookSyncServiceEndpoint, AppConfig.Current.OutlookMeetingCaptureClientTimeout)
		{
		}

		public OutlookMeetingCaptureClientWrapper(TimeSpan? timeout)
			: this(outlookSyncServiceEndpoint, timeout)
		{
		}

		public OutlookMeetingCaptureClientWrapper(string endpointAddress, TimeSpan? timeout)
		{
			Client = endpointAddress == null ? new MeetingCaptureServiceClient() : new MeetingCaptureServiceClient(new NetNamedPipeBinding() { MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = int.MaxValue } }, new EndpointAddress(endpointAddress));
			foreach (var operation in Client.Endpoint.Contract.Operations)
			{
				var behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
				if (behavior != null)
				{
					behavior.MaxItemsInObjectGraph = int.MaxValue;
				}
			}
			if (timeout.HasValue) SetTimeout(timeout.Value);
		}

		public void SetTimeout(TimeSpan timeout)
		{
			Client.Endpoint.Binding.OpenTimeout = timeout;
			Client.Endpoint.Binding.SendTimeout = timeout;
			Client.Endpoint.Binding.ReceiveTimeout = timeout;
			Client.OperationTimeout = timeout;
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
