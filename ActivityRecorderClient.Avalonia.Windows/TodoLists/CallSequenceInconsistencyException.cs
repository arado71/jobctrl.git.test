using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.TodoLists
{
	class CallSequenceInconsistencyException : Exception
	{
		public CallSequenceInconsistencyException(string message)
			: base(message)
		{
		}
	}
}
