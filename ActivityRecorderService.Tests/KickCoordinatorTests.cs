using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Kicks;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class KickCoordinatorTests
	{
		private readonly KickCoordinator kickCoordinator = new KickCoordinator();
		const int kickId = 2;

		[Fact]
		public void AddSetGetOrdered()
		{
			var task = Task.Factory.StartNew(() =>
				{
					kickCoordinator.AddKick(kickId);
				})
				.ContinueWith(ant =>
					kickCoordinator.TrySetKickResult(kickId, KickResult.Ok)
				)
				.ContinueWith<KickResult?>(ant =>
				{
					KickResult? result;
					return kickCoordinator.TryGetKickResult(kickId, out result) ? result : null;
				});
			Assert.Equal(KickResult.Ok, task.Result);
		}

		[Fact]
		public void AddGetSetOrdered()
		{
			var task = Task<KickResult?>.Factory.StartNew(() =>
				{
					kickCoordinator.AddKick(kickId);
					KickResult? result;
					return kickCoordinator.TryGetKickResult(kickId, out result) ? result : null;
				});
			Thread.Sleep(1000); //hax because I have no idea how to detect (easily) if TryGetKickResult is blocking...
			kickCoordinator.TrySetKickResult(kickId, KickResult.Ok);

			Assert.Equal(KickResult.Ok, task.Result);
		}

		[Fact]
		public void SetWithoutAdd()
		{
			Assert.False(kickCoordinator.TrySetKickResult(kickId, KickResult.Ok));
		}

		[Fact]
		public void GetWithoutAdd()
		{
			KickResult? result;
			Assert.False(kickCoordinator.TryGetKickResult(kickId, out result));
		}

		[Fact]
		public void SameAddWillThrow()
		{
			kickCoordinator.AddKick(kickId);
			Assert.Throws<ArgumentException>(() => kickCoordinator.AddKick(kickId));
		}
	}
}
