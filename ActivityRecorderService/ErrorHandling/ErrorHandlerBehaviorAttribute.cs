using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.ErrorHandling
{
	/// <summary>
	/// Class for converting exceptions to FaultExceptions in order not to Fault client channels
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ErrorHandlerBehaviorAttribute : Attribute, IErrorHandler, IServiceBehavior
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public bool HandleError(Exception error)
		{
			if (error is CommunicationException) return true;
			log.Debug("Service side error", error);
			//do nothing
			return false;  //execute next handler in the chain
		}

		//called first on the incoming call thread (should be fast)
		public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
		{
			if (error is FaultException) return;
			var ex = new FaultException("Internal server error");
			fault = Message.CreateMessage(version, ex.CreateMessageFault(), ex.Action);
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
			{
				dispatcher.ErrorHandlers.Add(this);
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
