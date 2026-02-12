using System;
using System.Diagnostics.Contracts;
using Reporter.Interfaces;

namespace Reporter.Contracts
{
	[ContractClassFor(typeof(IInterval))]
	internal abstract class ContractForIInterval : IInterval
	{
		public DateTime StartDate
		{
			get
			{
				Contract.Ensures(StartDate <= EndDate);
				return default(DateTime);
			}
		}

		public DateTime EndDate
		{
			get
			{
				Contract.Ensures(StartDate <= EndDate);
				return default(DateTime);
			}
		}

		[ContractInvariantMethod]
		private void IntervalInvariant()
		{
			Contract.Invariant(StartDate <= EndDate);
		}
	}
}
