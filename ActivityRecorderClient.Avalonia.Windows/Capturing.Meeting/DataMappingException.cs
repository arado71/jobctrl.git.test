using System;

namespace Tct.ActivityRecorderClient.Capturing.Meeting
{
	[Serializable]
	public class DataMappingException : Exception
	{
		public DataMappingException() { }
		public DataMappingException(string message) : base(message) { }
		public DataMappingException(string message, Exception inner) : base(message, inner) { }
		protected DataMappingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
