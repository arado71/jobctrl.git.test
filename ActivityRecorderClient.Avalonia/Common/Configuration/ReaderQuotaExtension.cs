using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
	/// <summary>
	/// https://canbilgin.wordpress.com/2010/06/25/how-to-set-maxitemsinobjectgraph-programmatically-for-client/
	/// </summary>
	public class ReaderQuotaExtension : IEndpointBehavior
	{
		private int maxItemsInObjectGraph;

		public ReaderQuotaExtension(int maxItemsInObjectGraph)
		{
			this.maxItemsInObjectGraph = maxItemsInObjectGraph;
		}

		#region Implementation of IEndpointBehavior

		public void Validate(ServiceEndpoint endpoint) { }

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			ModifyDataContractSerializerBehavior(endpoint);
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			ModifyDataContractSerializerBehavior(endpoint);
		}

		#endregion

		private void ModifyDataContractSerializerBehavior(ServiceEndpoint endpoint)
		{
			foreach (OperationDescription operation in endpoint.Contract.Operations)
			{
				var behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
				behavior.MaxItemsInObjectGraph = 2147483647;
			}
		}

	}
}
