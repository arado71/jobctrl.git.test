using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Telemetry.Data
{
	public class ExceptionData
	{
		public string Message { get; set; }
		public string Stack { get; set; }
		public ExceptionData Inner { get; set; }

		public ExceptionData() { }

		public ExceptionData(Exception ex)
		{
			Message = ex.Message;
			Stack = ex.StackTrace;
			if (ex.InnerException != null)
			{
				Inner = new ExceptionData(ex.InnerException);
			}
		}
	}
}
