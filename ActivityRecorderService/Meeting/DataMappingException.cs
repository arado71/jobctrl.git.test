using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Meeting
{
	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
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
