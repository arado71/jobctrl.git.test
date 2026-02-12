using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	/// <summary>
	/// Fix for .NET CLR IOCP thread pool bug, described here:
	/// https://docs.microsoft.com/en-US/troubleshoot/developer/dotnet/wcf/service-scale-up-slowly
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class WorkerThreadPoolBehaviorAttribute : Attribute, IContractBehavior
	{
		private static WorkerThreadPoolSynchronizer synchronizer = new WorkerThreadPoolSynchronizer();

		void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
		{
			dispatchRuntime.SynchronizationContext = synchronizer;
		}

		void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
		{
		}
	}

	public class WorkerThreadPoolSynchronizer : SynchronizationContext
	{
		public override void Post(SendOrPostCallback d, object state)
		{
			// WCF almost always uses Post
			ThreadPool.QueueUserWorkItem(new WaitCallback(d), state);
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			// Only the peer channel in WCF uses Send
			d(state);
		}
	}
}
