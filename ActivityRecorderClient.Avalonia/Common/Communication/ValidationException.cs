using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Communication
{
	public class ValidationException : Exception
	{
		public ValidationException(IEnumerable<ValidationResult> results)
		{
			Result = results;
			Severity = results.Max(x => x.Severity);
		}

		public Severity Severity { get; private set; }
		public IEnumerable<ValidationResult> Result { get; private set; } 
	}
}
