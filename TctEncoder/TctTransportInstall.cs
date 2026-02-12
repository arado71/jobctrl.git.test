using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Net.Security;

namespace TctTransport
{
	public class TctTransportElement : BindingElementExtensionElement
	{
		public static char[] s1c = TctTransportBindingElement.MouseDoubleClickSpeed.ToCharArray();
		public override Type BindingElementType																		//WCF calls it, independently from us, so it has to be here
		{
			get { return typeof(TctTransportElement); }
		}

		[ConfigurationProperty("transferMode", DefaultValue = "Buffered")]
		public string _transferMode
		{
			get { return (string)base["transferMode"]; }
			set { base["transferMode"] = value; }
		}

		[ConfigurationProperty("hostNameComparisonMode", DefaultValue = "StrongWildcard")]
		public string _hostNameComparisonMode
		{
			get { return (string)base["hostNameComparisonMode"]; }
			set { base["hostNameComparisonMode"] = value; }
		}

		[ConfigurationProperty("listenBacklog", DefaultValue = "10")]
		public string _listenBacklog
		{
			get { return (string)base["listenBacklog"]; }
			set { base["listenBacklog"] = value; }
		}

		[ConfigurationProperty("maxBufferPoolSize", DefaultValue = "524288")]
		public string _maxBufferPoolSize
		{
			get { return (string)base["maxBufferPoolSize"]; }
			set { base["maxBufferPoolSize"] = value; }
		}

		[ConfigurationProperty("maxBufferSize", DefaultValue = "65536")]
		public string _maxBufferSize
		{
			get { return (string)base["maxBufferSize"]; }
			set { base["maxBufferSize"] = value; }
		}

		[ConfigurationProperty("maxPendingConnections", DefaultValue = "10")]
		public string _maxPendingConnections
		{
			get { return (string)base["maxPendingConnections"]; }
			set { base["maxPendingConnections"] = value; }
		}

		[ConfigurationProperty("maxPendingAccepts", DefaultValue = "1")]
		public string _maxPendingAccepts
		{
			get { return (string)base["maxPendingAccepts"]; }
			set { base["maxPendingAccepts"] = value; }
		}

		[ConfigurationProperty("maxReceivedMessageSize", DefaultValue = "65536")]
		public string _maxReceivedMessageSize
		{
			get { return (string)base["maxReceivedMessageSize"]; }
			set { base["maxReceivedMessageSize"] = value; }
		}

		[ConfigurationProperty("portSharingEnabled", DefaultValue = "false")]
		public string _portSharingEnabled
		{
			get { return (string)base["portSharingEnabled"]; }
			set { base["portSharingEnabled"] = value; }
		}

		[ConfigurationProperty("groupName", DefaultValue = "default")]
		public string _groupName
		{
			get { return (string)base["groupName"]; }
			set { base["groupName"] = value; }
		}

		[ConfigurationProperty("idleTimeout", DefaultValue = "00:02:00")]
		public string _idleTimeout
		{
			get { return (string)base["idleTimeout"]; }
			set { base["idleTimeout"] = value; }
		}

		[ConfigurationProperty("leaseTimeout", DefaultValue = "00:05:00")]
		public string _leaseTimeout
		{
			get { return (string)base["leaseTimeout"]; }
			set { base["leaseTimeout"] = value; }
		}

		[ConfigurationProperty("maxOutboundConnectionsPerEndpoint", DefaultValue = "10")]
		public string _maxOutboundConnectionsPerEndpoint
		{
			get { return (string)base["maxOutboundConnectionsPerEndpoint"]; }
			set { base["maxOutboundConnectionsPerEndpoint"] = value; }
		}



		public override void ApplyConfiguration(BindingElement bindingElement)										
		{
			TctTransportBindingElement binding = (TctTransportBindingElement)bindingElement;
			PropertyInformationCollection propertyInfo = ElementInformation.Properties;
			if (propertyInfo["transferMode"].ValueOrigin != PropertyValueOrigin.Default)
				if (_transferMode == "Buffered") binding.TransferMode = TransferMode.Buffered;
				else if (_transferMode == "Streamed") binding.TransferMode = TransferMode.Streamed;
				else if (_transferMode == "StreamedRequest") binding.TransferMode = TransferMode.StreamedRequest;
				else if (_transferMode == "StreamedResponse") binding.TransferMode = TransferMode.StreamedResponse;
			if (propertyInfo["hostNameComparisonMode"].ValueOrigin != PropertyValueOrigin.Default)
				if (_hostNameComparisonMode == "Exact") binding.HostNameComparisonMode = HostNameComparisonMode.Exact;
				else if (_hostNameComparisonMode == "StrongWildcard") binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
				else if (_hostNameComparisonMode == "WeakWildcard") binding.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
			if (propertyInfo["listenBacklog"].ValueOrigin != PropertyValueOrigin.Default) binding.ListenBacklog = int.Parse(_listenBacklog);
			if (propertyInfo["maxBufferPoolSize"].ValueOrigin != PropertyValueOrigin.Default) binding.MaxBufferPoolSize = int.Parse(_maxBufferPoolSize);
			if (propertyInfo["maxBufferSize"].ValueOrigin != PropertyValueOrigin.Default) binding.MaxBufferSize = int.Parse(_maxBufferSize);
			if (propertyInfo["maxPendingConnections"].ValueOrigin != PropertyValueOrigin.Default) binding.MaxPendingConnections = int.Parse(_maxPendingConnections);
			if (propertyInfo["maxPendingAccepts"].ValueOrigin != PropertyValueOrigin.Default) binding.MaxPendingAccepts = int.Parse(_maxPendingAccepts);
			if (propertyInfo["maxReceivedMessageSize"].ValueOrigin != PropertyValueOrigin.Default) binding.MaxReceivedMessageSize = int.Parse(_maxReceivedMessageSize);
			if (propertyInfo["portSharingEnabled"].ValueOrigin != PropertyValueOrigin.Default) binding.PortSharingEnabled = (_portSharingEnabled.ToLower() == "true");
			if (propertyInfo["groupName"].ValueOrigin != PropertyValueOrigin.Default) binding.ConnectionPoolSettings.GroupName = _groupName;
			if (propertyInfo["idleTimeout"].ValueOrigin != PropertyValueOrigin.Default) binding.ConnectionPoolSettings.IdleTimeout = TimeSpan.Parse(_idleTimeout);
			if (propertyInfo["leaseTimeout"].ValueOrigin != PropertyValueOrigin.Default) binding.ConnectionPoolSettings.LeaseTimeout = TimeSpan.Parse(_leaseTimeout);
			if (propertyInfo["maxOutboundConnectionsPerEndpoint"].ValueOrigin != PropertyValueOrigin.Default) binding.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = int.Parse(_maxOutboundConnectionsPerEndpoint);
		}

		protected override BindingElement CreateBindingElement()													//WCF calls it, independently from us, so it has to be here
		{
			TctTransportBindingElement bindingElement = new TctTransportBindingElement();
			ApplyConfiguration(bindingElement);
			return bindingElement;
		}
	}

	public sealed class TctTransportBindingElement : TcpTransportBindingElement 									//it is called by the CreateBindingElement method of the previous class
	{
		public const string MouseDoubleClickSpeed = "AhgWeuTRqoiUPQwW";
		public override BindingElement Clone()																		//WCF calls it, independently from us, so it has to be here
		{
			TctTransportBindingElement bindingElement = new TctTransportBindingElement();
			bindingElement.TransferMode = TransferMode;
			bindingElement.HostNameComparisonMode = HostNameComparisonMode;
			bindingElement.ListenBacklog = ListenBacklog;
			bindingElement.MaxBufferPoolSize = MaxBufferPoolSize;
			bindingElement.MaxBufferSize = MaxBufferSize;
			bindingElement.MaxPendingConnections = MaxPendingConnections;
			bindingElement.MaxPendingAccepts = MaxPendingAccepts;
			bindingElement.MaxReceivedMessageSize = MaxReceivedMessageSize;
			bindingElement.PortSharingEnabled = PortSharingEnabled;
			bindingElement.ConnectionPoolSettings.GroupName = ConnectionPoolSettings.GroupName;
			bindingElement.ConnectionPoolSettings.IdleTimeout = ConnectionPoolSettings.IdleTimeout;
			bindingElement.ConnectionPoolSettings.LeaseTimeout = ConnectionPoolSettings.LeaseTimeout;
			bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint;
			return bindingElement;
		}

		public override T GetProperty<T>(BindingContext context)													//WCF calls it, independently from us, so it has to be here
		{
			if (typeof(T) == typeof(ISecurityCapabilities)) return (T)(object)new TctSecurityCapabilities();
			else return base.GetProperty<T>(context);
		}
	}

	public class TctSecurityCapabilities : ISecurityCapabilities
	{
		public ProtectionLevel SupportedRequestProtectionLevel { get { return ProtectionLevel.EncryptAndSign; } }
		public ProtectionLevel SupportedResponseProtectionLevel { get { return ProtectionLevel.EncryptAndSign; } }
		public bool SupportsClientAuthentication { get { return false; } }
		public bool SupportsClientWindowsIdentity { get { return false; } }
		public bool SupportsServerAuthentication { get { return true; } }
	}
}
