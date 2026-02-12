using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookMeetingCaptureService
{
	[Serializable]
	public class SmtpAddressNotFoundException : Exception
	{
		public SmtpAddressNotFoundException() { }
		public SmtpAddressNotFoundException(string message) : base(message) { }
		public SmtpAddressNotFoundException(string message, Exception inner) : base(message, inner) { }
		protected SmtpAddressNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
