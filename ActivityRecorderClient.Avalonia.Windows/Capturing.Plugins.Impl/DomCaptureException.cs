using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	[Serializable]
	public class DomCaptureException : Exception
	{
		public DomCaptureException()
		{
		}

		public DomCaptureException(string message)
			: base(message)
		{
		}

		public DomCaptureException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected DomCaptureException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}
	}
}
