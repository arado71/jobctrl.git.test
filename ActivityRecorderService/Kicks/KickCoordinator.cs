using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService.Kicks
{
	/// <summary>
	/// Thread-safe class for coordinating waits for kicks
	/// </summary>
	public class KickCoordinator
	{
		private readonly Dictionary<int, KickResultWaiter> followedKicks = new Dictionary<int, KickResultWaiter>();
		private readonly object thisLock = new object();

		public void AddKick(int kickId)
		{
			lock (thisLock)
			{
				followedKicks.Add(kickId, new KickResultWaiter());
			}
		}

		public bool TrySetKickResult(int kickId, KickResult? result)
		{
			KickResultWaiter waiter;
			lock (thisLock)
			{
				if (followedKicks.TryGetValue(kickId, out waiter))
				{
					waiter.SetResult(result);
					return true;
				}
			}
			return false;
		}

		public bool TryGetKickResult(int kickId, out KickResult? result)
		{
			KickResultWaiter waiter;
			lock (thisLock)
			{
				if (!followedKicks.TryGetValue(kickId, out waiter))
				{
					result = null;
					return false;
				}
			}
			result = waiter.GetResult(); //long running
			lock (thisLock)
			{
				followedKicks.Remove(kickId); //remove on successful get (there is a race when there are multiple getters but there won't be and it doesn't matter anyway)
			}
			return true;
		}

		private class KickResultWaiter
		{
			private readonly ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);
			private KickResult? kickResult;

			public KickResult? GetResult()
			{
				waitHandle.Wait();
				return kickResult;
			}

			public void SetResult(KickResult? result)
			{
				kickResult = result;
				waitHandle.Set();
			}
		}
	}
}
