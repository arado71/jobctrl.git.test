using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Menu
{
	[Serializable]
	public class LearnedUserWorkSwitch
	{
		public int WorkId { get; set; }
		public int?[] ListPositions { get; set; }
		public DateTime Date { get; set; }
	}
}
