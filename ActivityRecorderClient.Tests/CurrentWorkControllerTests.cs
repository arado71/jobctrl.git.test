using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Controller;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class CurrentWorkControllerTests
	{
		private static CurrentWorkControllerBaseTester GetCurrentWorkController()
		{
			var mock = new Mock<CurrentWorkControllerBaseTester>() { CallBase = true };
			//mock.Setup(ctx => ctx.CurrentWork).Returns(null);
			mock.Object.Mock = mock;
			return mock.Object;
		}

		#region Create
		[Fact]
		public void CanCreateController()
		{
			Assert.NotNull(GetCurrentWorkController());
		}

		[Fact]
		public void DefaultWorkStateNotWorking()
		{
			//Arrange
			var controller = GetCurrentWorkController();
			//Act
			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Assert.Equal((WorkStateChangeType)0, controller.LastWorkStateChangeType);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Never());
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}
		#endregion

		#region Resume
		[Fact]
		public void CannotResumeAtStart()
		{
			//Arrange
			var controller = GetCurrentWorkController();
			//controller.Mock.Setup(ctx => ctx.ProtectedStartWork(It.IsAny<int>())).Throws(new Exception("this should be not called"));

			//Act
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Assert.Equal((WorkStateChangeType)0, controller.LastWorkStateChangeType);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Never());
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void CannotResumeWhenWorking()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void CannotResumeWhenWorkingTemp()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.WorkingTemp, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedTempStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedTemp, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void ResumeUserSelectedWork()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.UserStopWork();
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedResumeWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserResume, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}

		[Fact]
		public void ResumeUserSelectedWorkWhenWorkingTempBeforeStop()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.UserStopWork();
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedResumeWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserResume, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}
		#endregion

		#region UserStart
		[Fact]
		public void StartUserSelectedWork()
		{
			//Arrange
			var work = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work);

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void StartSeveralUserSelectedWorks()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var work3 = new WorkData() { Id = 3 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.UserStartWork(work3);
			controller.UserStartWork(work2);
			controller.UserStartWork(work3);
			controller.UserStartWork(work2);
			controller.UserStartWork(work3);

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work3, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work3.Id.Value), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(6));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}
		#endregion

		#region UserStop
		[Fact]
		public void StartStopSeveralUserSelectedWorks()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var work3 = new WorkData() { Id = 3 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.UserStartWork(work3);
			controller.UserStopWork();
			controller.UserStartWork(work2);
			controller.UserStartWork(work3);
			controller.UserStopWork();
			controller.UserStartWork(work2);
			controller.UserStartWork(work3);
			controller.UserStopWork();

			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStopWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work3.Id.Value), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(6));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(3));
		}

		[Fact]
		public void StopWhenWorkingTemp()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.UserStopWork();

			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStopWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.WorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}
		#endregion

		#region TempStart
		[Fact]
		public void CannotStartWorkWithTempStartWork()
		{
			//Arrange
			var work = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.TempStartWork(work);

			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Assert.Equal((WorkStateChangeType)0, controller.LastWorkStateChangeType);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Never());
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void CanTempStartWorkWhenWorking()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);

			//Assert
			Assert.Equal(WorkState.WorkingTemp, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedTempStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedTemp, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void TempEndEffectSwitchBackToOriginalWorkWhenWorkingTemp()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedEndTempStartWorkStartUserWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedEndTempEffect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.WorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void TempEndEffectDoNothingWhenWorking()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		public void TempEndEffectDoNothingWhenWorkingAgain()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.UserStartWork(work1);
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.UserSelectedStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.WorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}
		#endregion

		#region TempStop
		[Fact]
		public void TempStopWorkWhenWorking()
		{
			//Arrange
			var work = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work);
			controller.TempStopWork();

			//Assert
			Assert.Equal(WorkState.NotWorkingTemp, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedTempStopWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedTemp, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}

		[Fact]
		public void TempStopWorkWhenWorkingTemp()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.TempStopWork();

			//Assert
			Assert.Equal(WorkState.NotWorkingTemp, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedTempStopWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedTemp, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.WorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}


		[Fact]
		public void TempEndEffectWhenNotWorkingTempButWorkingTempBefore()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStartWork(work2);
			controller.TempStopWork();
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedEndTempStopWorkStartUserWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedEndTempEffect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}

		[Fact]
		public void TempEndEffectWhenNotWorkingTempButWorkingBefore()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStopWork();
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedEndTempStopWorkStartUserWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedEndTempEffect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}

		[Fact]
		public void TempStartWorkWhenNotWorkingTempButWorkingBefore()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.TempStopWork();
			controller.TempStartWork(work2);

			//Assert
			Assert.Equal(WorkState.WorkingTemp, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeType.AutodetectedEndTempStopWorkSartTempStartWork, controller.LastWorkStateChangeType);
			Assert.Equal(WorkStateChangeReason.AutodetectedTemp, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}
		#endregion

		#region PermStart
		[Fact]
		public void CannotStartWorkWithPermStartWork()
		{
			//Arrange
			var work = new WorkData() { Id = 1 };
			var controller = GetCurrentWorkController();

			//Act
			controller.PermStartWork(work);

			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Never());
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void CanPermStartWorkWhenWorking()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.AutodetectedPerm, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void TempEndEffectWontSwitchBackToOriginalWorkWhenWorkingPerm()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.AutodetectedPerm, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void TempEndEffectDoNothingWhenWorkingAgainAfterPerm()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);
			controller.UserStartWork(work1);
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work1, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void CanStopPermStartWork()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);
			controller.UserStopWork();

			//Assert
			Assert.Equal(WorkState.NotWorking, controller.CurrentWorkState);
			Assert.Equal(null, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserSelect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.Working, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}

		[Fact]
		public void TempEndEffectCanSwitchBackToPermStartWork()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);
			controller.TempStartWork(work1);
			controller.TempEndEffect();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.AutodetectedEndTempEffect, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.WorkingTemp, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(4));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Never());
		}

		[Fact]
		public void CanResumePermStartWork()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);
			controller.UserStopWork();
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserResume, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(3));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}

		[Fact]
		public void CanResumePermStartWorkButNotTempStart()
		{
			//Arrange
			var work1 = new WorkData() { Id = 1 };
			var work2 = new WorkData() { Id = 2 };
			var work3 = new WorkData() { Id = 3 };
			var controller = GetCurrentWorkController();

			//Act
			controller.UserStartWork(work1);
			controller.PermStartWork(work2);
			controller.TempStartWork(work3);
			controller.UserStopWork();
			controller.UserResumeWork();

			//Assert
			Assert.Equal(WorkState.Working, controller.CurrentWorkState);
			Assert.Equal(work2, controller.CurrentWork);
			Assert.Equal(WorkStateChangeReason.UserResume, controller.LastWorkStateChangeReason);
			Assert.Equal(WorkState.NotWorking, controller.LastWorkState);
			//Mock Assert
			controller.Mock.Verify(n => n.ProtectedStartWork(work1.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work3.Id.Value), Times.Exactly(1));
			controller.Mock.Verify(n => n.ProtectedStartWork(work2.Id.Value), Times.Exactly(2));
			controller.Mock.Verify(n => n.ProtectedStartWork(It.IsAny<int>()), Times.Exactly(4));
			controller.Mock.Verify(n => n.ProtectedStopWork(), Times.Exactly(1));
		}
		#endregion

	}

	public class CurrentWorkControllerBaseTester : CurrentWorkControllerBase
	{
		public Mock<CurrentWorkControllerBaseTester> Mock { get; set; }

		public virtual void ProtectedStopWork()
		{
		}

		public virtual void ProtectedStartWork(int id)
		{
		}

		protected override void StopWork()
		{
			ProtectedStopWork();
		}

		protected override void StartWork(WorkData workData)
		{
			ProtectedStartWork(workData.Id.Value);
		}

		public new WorkState LastWorkState { get { return base.LastWorkState; } }

		public new WorkStateChangeReason LastWorkStateChangeReason { get { return base.LastWorkStateChangeReason; } }


		//legacy states
		public WorkStateChangeType LastWorkStateChangeType
		{
			get
			{
				if (CurrentWorkState == WorkState.Working && LastWorkStateChangeReason == WorkStateChangeReason.UserSelect) return WorkStateChangeType.UserSelectedStartWork;
				if (CurrentWorkState == WorkState.NotWorking && LastWorkStateChangeReason == WorkStateChangeReason.UserSelect) return WorkStateChangeType.UserSelectedStopWork;
				if (CurrentWorkState == WorkState.Working && LastWorkStateChangeReason == WorkStateChangeReason.UserResume) return WorkStateChangeType.UserSelectedResumeWork;
				if (CurrentWorkState == WorkState.NotWorkingTemp) return WorkStateChangeType.AutodetectedTempStopWork;
				if (CurrentWorkState == WorkState.Working && LastWorkState == WorkState.NotWorkingTemp && LastWorkStateChangeReason == WorkStateChangeReason.AutodetectedEndTempEffect) return WorkStateChangeType.AutodetectedEndTempStopWorkStartUserWork;
				if (CurrentWorkState == WorkState.Working && LastWorkState == WorkState.WorkingTemp && LastWorkStateChangeReason == WorkStateChangeReason.AutodetectedEndTempEffect) return WorkStateChangeType.AutodetectedEndTempStartWorkStartUserWork;
				if (CurrentWorkState == WorkState.WorkingTemp && LastWorkState == WorkState.NotWorkingTemp && (LastWorkStateChangeReason == WorkStateChangeReason.AutodetectedEndTempEffect || LastWorkStateChangeReason == WorkStateChangeReason.AutodetectedTemp)) return WorkStateChangeType.AutodetectedEndTempStopWorkSartTempStartWork;
				if (CurrentWorkState == WorkState.WorkingTemp && LastWorkStateChangeReason == WorkStateChangeReason.AutodetectedTemp) return WorkStateChangeType.AutodetectedTempStartWork;
				throw null;
			}
		}
	}

	public enum WorkStateChangeType
	{
		UserSelectedStartWork = 0,
		UserSelectedStopWork,
		UserSelectedResumeWork,
		AutodetectedTempStartWork,
		AutodetectedTempStopWork,
		AutodetectedEndTempStopWorkStartUserWork,
		AutodetectedEndTempStartWorkStartUserWork,
		AutodetectedEndTempStopWorkSartTempStartWork,
	}
}
