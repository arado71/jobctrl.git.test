using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ClientErrorReporting
{
	public class ReportingProgress
	{
		public int NumberOfPhases { get; set; }

		public int CurrentPhase { get; set; }

		public string PhaseText { get; set; }

		public int Value { get; set; }
	}
}
