using System;
using System.Diagnostics.Contracts;
using Reporter.Contracts;

namespace Reporter.Interfaces
{
	[ContractClass(typeof(ContractForICollectedItem))]
	public interface ICollectedItem
	{
		DateTime CreateDate { get; }
		string Key { get; }
		string Value { get; }
		int UserId { get; }

	}
}
