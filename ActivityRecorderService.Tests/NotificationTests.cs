using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.Notifications;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class NotificationTests : DbTestsBase
	{
		private static readonly DateTime now = new DateTime(2014, 04, 07, 12, 00, 00);
		private const int companyId = 1;
		private const int workId = 2;
		private const int userId = 3;
		private const int createdBy = 4;
		private const int computerId = 5;

		private static NotificationForm GetNotificationForm(int pUserId = userId, int pCompanyId = companyId, int? pWorkId = workId)
		{
			return new NotificationForm()
			{
				Name = "Test",
				CompanyId = pCompanyId,
				WorkId = pWorkId,
				Data = NotificationService.SerializeData(new JcForm()
				{
					CloseButtonId = "Cancel",
					BeforeShowActions = new List<string>() { CustomActions.RefreshMenu, },
					MessageBox = new JcMessageBox()
					{
						Title = "Title",
						Text = "Text",
						Buttons = new List<JcButton>()
						{
							new JcButton() { Id = "Ok", Text = "Ok" },
							new JcButton() { Id = "Cancel", Text = "Cancel" },
						}
					},
				}),
				ClientNotifications = new System.Data.Linq.EntitySet<ClientNotification>()
				{
					new ClientNotification()
					{
						UserId = pUserId,
						CreatedBy = createdBy,
					},
				},
			};
		}

		[Fact]
		public NotificationForm CreateForm()
		{
			//Arrange
			var form = GetNotificationForm();
			using (var context = new NotificationDataClassesDataContext())
			{
				context.NotificationForms.InsertOnSubmit(form);
				//Act
				context.SubmitChanges();
			}

			//Assert
			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Null(notif.ConfirmDate);
				Assert.Null(notif.DeviceId);
				Assert.Null(notif.ReceiveDate);
				Assert.Null(notif.Result);
				Assert.Null(notif.SendDate);
				Assert.Null(notif.ShowDate);
				var dbForm = notif.NotificationForm;
				Assert.Equal("Test", dbForm.Name);
				Assert.Equal(workId, dbForm.WorkId);
				Assert.Equal(companyId, dbForm.CompanyId);
				Assert.Null(dbForm.DeleteDate);
			}

			return form;
		}

		[Fact]
		public ClientNotification GetPending()
		{
			return GetPendingFromCreated(null);
		}

		public ClientNotification GetPendingFromCreated(NotificationForm createdForm)
		{
			//Arrange
			var form = createdForm ?? CreateForm();
			var svc = new NotificationService();

			//Act
			var data = svc.GetPendingNotification(userId, computerId, null);

			//Assert
			Assert.NotNull(data);
			Assert.Equal(form.Id, data.FormId);
			Assert.Equal(form.WorkId, data.WorkId);
			Assert.Equal(form.Name, data.Name);

			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Null(notif.ConfirmDate);
				Assert.Equal(computerId, notif.DeviceId);
				Assert.Null(notif.ReceiveDate);
				Assert.Null(notif.Result);
				Assert.NotNull(notif.SendDate);
				Assert.Null(notif.ShowDate);
				return notif;
			}
		}

		[Fact]
		public void GetPendingTwoTimes()
		{
			var form = CreateForm();
			GetPendingFromCreated(form);
			GetPendingFromCreated(form);
		}

		[Fact]
		public void GetPendingTwoTimesLastId()
		{
			//Arrange
			var form = CreateForm();
			var pend = GetPendingFromCreated(form);
			var svc = new NotificationService();

			//Act
			var data = svc.GetPendingNotification(userId, computerId, pend.Id);

			//Assert
			Assert.Null(data);
		}

		[Fact]
		public void GetPendingLastId()
		{
			//Arrange
			CreateForm();
			var svc = new NotificationService();

			//Act
			var data = svc.GetPendingNotification(userId, computerId, 1000);

			//Assert
			Assert.Null(data);
		}

		[Fact(Skip = "This is slow (18s) that is why we need to cache notifications")]
		public void GetPendingPerformance()
		{
			const int it = 1000;

			//Arrange
			var st = Environment.TickCount;
			var svc = new NotificationService();

			for (int i = 0; i < it; i++)
			{
				//Act
				var data = svc.GetPendingNotification(userId, computerId, null);

				//Assert
				Assert.Null(data);
			}

			Console.WriteLine(Environment.TickCount - st);
		}


		[Fact]
		public void CannotGetPendingWrongUser()
		{
			//Arrange
			CreateForm();
			var svc = new NotificationService();

			//Act
			var data = svc.GetPendingNotification(userId + 1, computerId, null);

			//Assert
			Assert.Null(data);

			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Null(notif.ConfirmDate);
				Assert.Null(notif.DeviceId);
				Assert.Null(notif.ReceiveDate);
				Assert.Null(notif.Result);
				Assert.Null(notif.SendDate);
				Assert.Null(notif.ShowDate);
			}
		}

		[Fact]
		public ClientNotification GetPendingLostRaceToOtherDevice()
		{
			//Arrange
			GetPending();
			var svc = new NotificationService();

			//Act
			var data = svc.GetPendingNotification(userId, computerId + 1, null);

			//Assert
			Assert.Null(data);

			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Null(notif.ConfirmDate);
				Assert.Equal(computerId, notif.DeviceId);
				Assert.Null(notif.ReceiveDate);
				Assert.Null(notif.Result);
				Assert.NotNull(notif.SendDate);
				Assert.Null(notif.ShowDate);
				return notif;
			}
		}

		[Fact]
		public ClientNotification Confirm()
		{
			//Arrange
			var data = GetPending();
			var svc = new NotificationService();

			//Act
			svc.ConfirmNotification(new NotificationResult()
			{
				Id = data.Id,
				Result = "Ok",
				UserId = userId,
				ShowDate = now,
				ConfirmDate = now.AddMinutes(1),
			});

			//Assert
			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Equal(now.AddMinutes(1), notif.ConfirmDate);
				Assert.Equal(computerId, notif.DeviceId);
				Assert.NotNull(notif.ReceiveDate);
				Assert.Equal("Ok", notif.Result);
				Assert.NotNull(notif.SendDate);
				Assert.Equal(now, notif.ShowDate);
				return notif;
			}
		}

		[Fact]
		public ClientNotification ConfirmTruncated()
		{
			//Arrange
			var data = GetPending();
			var svc = new NotificationService();

			//Act
			svc.ConfirmNotification(new NotificationResult()
			{
				Id = data.Id,
				Result = new string('a', 20000),
				UserId = userId,
				ShowDate = now,
				ConfirmDate = now.AddMinutes(1),
			});

			//Assert
			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Equal(now.AddMinutes(1), notif.ConfirmDate);
				Assert.Equal(computerId, notif.DeviceId);
				Assert.NotNull(notif.ReceiveDate);
				Assert.True(notif.Result.Length < 20000 && notif.Result.Length > 0 && notif.Result.Replace("a", "").Length == 0);
				Assert.NotNull(notif.SendDate);
				Assert.Equal(now, notif.ShowDate);
				return notif;
			}
		}

		[Fact]
		public ClientNotification CannotOverwriteConfirm()
		{
			//Arrange
			var data = Confirm();
			var svc = new NotificationService();

			//Act
			svc.ConfirmNotification(new NotificationResult()
			{
				Id = data.Id,
				Result = "Cancel",
				UserId = userId,
				ShowDate = data.ShowDate.Value.AddDays(-1),
				ConfirmDate = data.ConfirmDate.Value.AddDays(-1),
			});

			//Assert
			using (var context = new NotificationDataClassesDataContext())
			{
				var notif = context.ClientNotifications.Single();
				Assert.NotNull(notif.CreateDate);
				Assert.Equal(createdBy, notif.CreatedBy);
				Assert.Equal(userId, notif.UserId);
				Assert.Equal(now.AddMinutes(1), notif.ConfirmDate);
				Assert.Equal(computerId, notif.DeviceId);
				Assert.NotNull(notif.ReceiveDate);
				Assert.Equal("Ok", notif.Result);
				Assert.NotNull(notif.SendDate);
				Assert.Equal(now, notif.ShowDate);
				return notif;
			}
		}

		[Fact]
		public void CannotConfirmWrongUser()
		{
			//Arrange
			var data = GetPending();
			var svc = new NotificationService();

			//Act
			//Assert
			Assert.Throws<InvalidOperationException>(() =>
				svc.ConfirmNotification(new NotificationResult()
				{
					Id = data.Id,
					Result = "Ok",
					UserId = userId + 1,
					ShowDate = now,
					ConfirmDate = now.AddMinutes(1),
				}));
		}
	}
}
