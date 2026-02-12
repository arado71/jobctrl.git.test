using System;
using System.Diagnostics.Contracts;
using Reporter.Contracts;

namespace Reporter.Interfaces
{
	[ContractClass(typeof(ContractForIInterval))]
	public interface IInterval
	{
		DateTime StartDate { get; }
		DateTime EndDate { get; }
	}
}
