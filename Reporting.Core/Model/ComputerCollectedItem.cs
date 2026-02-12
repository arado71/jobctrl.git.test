using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Model
{
	public class ComputerCollectedItem : CollectedItem, IComputerCollectedItem
	{
		public int ComputerId { get; set; }
	}
}
