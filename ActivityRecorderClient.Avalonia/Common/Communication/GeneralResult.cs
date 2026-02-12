using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Communication
{
	public class GeneralResult<T>
	{
		public T Result { get; set; }
		public Exception Exception { get; set; }

		public GeneralResult()
		{
		}

		public GeneralResult(Func<T> funcToExecute)
		{
			try
			{
				Result = funcToExecute();
			}
			catch (Exception e)
			{
				Exception = e;
			}
		}
	}
}
