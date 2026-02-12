using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Contracts
{
	[ContractClassFor(typeof(ICollectedItem))]
	internal abstract class ContractForICollectedItem : ICollectedItem
	{
		public DateTime CreateDate { get; private set; }
		public string Key {
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return "";
			}
		}
		public string Value { get; private set; }
		public int UserId { get; private set; }
	}
}
