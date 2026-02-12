using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Persistence;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ClientSettingsTests : DbTestsBase
	{
		[Fact]
		public void IsGenerateAllInSyncWithLinqClass()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
				});
				context.SubmitChanges();
			}

			//Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				Assert.NotNull(context.ClientSettings.Where(n => n.UserId == 13).Single());
			}
		}

		[Fact]
		public void VersionsUpdatedOnInsert()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting() { UserId = 1 });
				context.SubmitChanges();
			}

			ClientSetting clientSettingDb;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				clientSettingDb = context.ClientSettings.Single(n => n.UserId == 1);
			}

			//Assert
			Assert.NotNull(clientSettingDb);
			Assert.NotEqual(0, clientSettingDb.MenuVersion.ToLong());
			Assert.NotEqual(0, clientSettingDb.WorkDetectorRulesVersion.ToLong());
			Assert.NotEqual(0, clientSettingDb.CensorRulesVersion.ToLong());
			Assert.NotEqual(0, clientSettingDb.ClientSettingsVersion.ToLong());
			Assert.NotEqual(0, clientSettingDb.CollectorRulesVersion.ToLong());
		}

		[Fact]
		public void VersionsUpdatedOnMultiRowInsert()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand("INSERT INTO [dbo].[ClientSettings] ([UserId]) VALUES (@p0), (@p1), (@p2)", 1, 2, 3);
				//context.SubmitChanges();
			}

			List<ClientSetting> clientSettingsDb;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				clientSettingsDb = context.ClientSettings.ToList();
			}

			//Assert
			foreach (var cs in clientSettingsDb)
			{
				Assert.NotNull(cs);
				Assert.NotEqual(0, cs.MenuVersion.ToLong());
				Assert.NotEqual(0, cs.WorkDetectorRulesVersion.ToLong());
				Assert.NotEqual(0, cs.CensorRulesVersion.ToLong());
				Assert.NotEqual(0, cs.ClientSettingsVersion.ToLong());
				Assert.NotEqual(0, cs.CollectorRulesVersion.ToLong());
			}
		}

		[Fact]
		public void MenuVersionIncrementedOnMenuUpdateFromNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1 };
			var edited = InsertThenUpdate(orig, n => n.Menu = "Edited");

			//Assert
			Assert.True(orig.MenuVersion.ToLong() < edited.MenuVersion.ToLong());
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void MenuVersionIncrementedOnMenuUpdate()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, Menu = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.Menu = "Edited");

			//Assert
			Assert.True(orig.MenuVersion.ToLong() < edited.MenuVersion.ToLong());
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void MenuVersionIncrementedOnMenuUpdateToNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, Menu = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.Menu = null);

			//Assert
			Assert.True(orig.MenuVersion.ToLong() < edited.MenuVersion.ToLong());
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void MenuVersionNotIncrementedOnMenuUpdateToSameValue()
		{
			//Arrange	//Update Menu from null to null
			var orig1 = new ClientSetting() { UserId = 1 };
			var edited1 = InsertThenExecuteCommand(orig1, "UPDATE [dbo].[ClientSettings] SET [Menu] = NULL WHERE [UserId] = @p0", 1);	//Can't pass in null as parameter. (null, (string)null, default(string), DBNull.Value)

			//Assert
			Assert.Equal(orig1.MenuVersion, edited1.MenuVersion);
			Assert.Equal(orig1.WorkDetectorRulesVersion, edited1.WorkDetectorRulesVersion);
			Assert.Equal(orig1.CensorRulesVersion, edited1.CensorRulesVersion);
			Assert.Equal(orig1.CollectorRulesVersion, edited1.CollectorRulesVersion);
			Assert.Equal(orig1.ClientSettingsVersion, edited1.ClientSettingsVersion);

			//Arrange	//Update Menu from non null value to same non null value
			var orig2 = new ClientSetting() { UserId = 2, Menu = "InitialValue" };
			var edited2 = InsertThenExecuteCommand(orig2, "UPDATE [dbo].[ClientSettings] SET [Menu] = @p0 WHERE [UserId] = @p1", "InitialValue", 2);

			//Assert
			Assert.Equal(orig2.MenuVersion, edited2.MenuVersion);
			Assert.Equal(orig2.WorkDetectorRulesVersion, edited2.WorkDetectorRulesVersion);
			Assert.Equal(orig2.CensorRulesVersion, edited2.CensorRulesVersion);
			Assert.Equal(orig2.CollectorRulesVersion, edited2.CollectorRulesVersion);
			Assert.Equal(orig2.ClientSettingsVersion, edited2.ClientSettingsVersion);
		}

		[Fact]
		public void MenuVersionIncrementedOnMultiRowMenuUpdate()	//TODO: Tests for multirow update for WorkDetectorRules, CensorRules, ClientSettings
		{
			List<ClientSetting> orig;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand("INSERT INTO [dbo].[ClientSettings] ([UserId], [Menu]) VALUES (@p0, @p1), (@p2, @p3), (@p4, @p5)", 1, "InitialValue", 2, "InitialValue", 3, "InitialValue");
				orig = context.ClientSettings.ToList();
			}

			List<ClientSetting> edited;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ExecuteCommand("UPDATE [dbo].[ClientSettings] SET [Menu] = @p0", "Edited");
				edited = context.ClientSettings.ToList();
			}

			//Assert
			foreach (var o in orig)
			{
				var tmp = o;
				var e = edited.Single(n => n.UserId == tmp.UserId);
				Assert.NotNull(o);
				Assert.True(o.MenuVersion.ToLong() < e.MenuVersion.ToLong());
				Assert.Equal(o.WorkDetectorRulesVersion, e.WorkDetectorRulesVersion);
				Assert.Equal(o.CensorRulesVersion, e.CensorRulesVersion);
				Assert.Equal(o.CollectorRulesVersion, e.CollectorRulesVersion);
				Assert.Equal(o.ClientSettingsVersion, e.ClientSettingsVersion);
			}
		}

		[Fact]
		public void WorkDetectorRulesVersionIncrementedOnWorkDetectorRulesUpdateFromNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1 };
			var edited = InsertThenUpdate(orig, n => n.WorkDetectorRules = "Edited");

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.True(orig.WorkDetectorRulesVersion.ToLong() < edited.WorkDetectorRulesVersion.ToLong());
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void WorkDetectorRulesVersionIncrementedOnWorkDetectorRulesUpdate()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, WorkDetectorRules = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.WorkDetectorRules = "Edited");

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.True(orig.WorkDetectorRulesVersion.ToLong() < edited.WorkDetectorRulesVersion.ToLong());
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void WorkDetectorRulesVersionIncrementedOnWorkDetectorRulesUpdateToNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, WorkDetectorRules = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.WorkDetectorRules = null);

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.True(orig.WorkDetectorRulesVersion.ToLong() < edited.WorkDetectorRulesVersion.ToLong());
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void WorkDetectorRulesVersionNotIncrementedOnWorkDetectorRulesUpdateToSameValue()
		{
			//Arrange	//Update Menu from null to null
			var orig1 = new ClientSetting() { UserId = 1 };
			var edited1 = InsertThenExecuteCommand(orig1, "UPDATE [dbo].[ClientSettings] SET [WorkDetectorRules] = NULL WHERE [UserId] = @p0", 1);	//Can't pass in null as parameter. (null, (string)null, default(string), DBNull.Value)

			//Assert
			Assert.Equal(orig1.MenuVersion, edited1.MenuVersion);
			Assert.Equal(orig1.WorkDetectorRulesVersion, edited1.WorkDetectorRulesVersion);
			Assert.Equal(orig1.CensorRulesVersion, edited1.CensorRulesVersion);
			Assert.Equal(orig1.CollectorRulesVersion, edited1.CollectorRulesVersion);
			Assert.Equal(orig1.ClientSettingsVersion, edited1.ClientSettingsVersion);

			//Arrange	//Update Menu from non null value to same non null value
			var orig2 = new ClientSetting() { UserId = 2, WorkDetectorRules = "InitialValue" };
			var edited2 = InsertThenExecuteCommand(orig2, "UPDATE [dbo].[ClientSettings] SET [WorkDetectorRules] = @p0 WHERE [UserId] = @p1", "InitialValue", 2);

			//Assert
			Assert.Equal(orig2.MenuVersion, edited2.MenuVersion);
			Assert.Equal(orig2.WorkDetectorRulesVersion, edited2.WorkDetectorRulesVersion);
			Assert.Equal(orig2.CensorRulesVersion, edited2.CensorRulesVersion);
			Assert.Equal(orig2.CollectorRulesVersion, edited2.CollectorRulesVersion);
			Assert.Equal(orig2.ClientSettingsVersion, edited2.ClientSettingsVersion);
		}

		[Fact]
		public void CensorRulesVersionIncrementedOnCensorRulesUpdateFromNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1 };
			var edited = InsertThenUpdate(orig, n => n.CensorRules = "Edited");

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.True(orig.CensorRulesVersion.ToLong() < edited.CensorRulesVersion.ToLong());
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void CensorRulesVersionIncrementedOnCensorRulesUpdate()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, CensorRules = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.CensorRules = "Edited");

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.True(orig.CensorRulesVersion.ToLong() < edited.CensorRulesVersion.ToLong());
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void CensorRulesVersionIncrementedOnCensorRulesUpdateToNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, CensorRules = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.CensorRules = null);

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.True(orig.CensorRulesVersion.ToLong() < edited.CensorRulesVersion.ToLong());
			Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
		}

		[Fact]
		public void CensorRulesVersionNotIncrementedOnCensorRulesUpdateToSameValue()
		{
			//Arrange	//Update Menu from null to null
			var orig1 = new ClientSetting() { UserId = 1 };
			var edited1 = InsertThenExecuteCommand(orig1, "UPDATE [dbo].[ClientSettings] SET [CensorRules] = NULL WHERE [UserId] = @p0", 1);	//Can't pass in null as parameter. (null, (string)null, default(string), DBNull.Value)


			//Assert
			Assert.Equal(orig1.MenuVersion, edited1.MenuVersion);
			Assert.Equal(orig1.WorkDetectorRulesVersion, edited1.WorkDetectorRulesVersion);
			Assert.Equal(orig1.CensorRulesVersion, edited1.CensorRulesVersion);
			Assert.Equal(orig1.CollectorRulesVersion, edited1.CollectorRulesVersion);
			Assert.Equal(orig1.ClientSettingsVersion, edited1.ClientSettingsVersion);

			//Arrange	//Update Menu from non null value to same non null value
			var orig2 = new ClientSetting() { UserId = 2, CensorRules = "InitialValue" };
			var edited2 = InsertThenExecuteCommand(orig2, "UPDATE [dbo].[ClientSettings] SET [CensorRules] = @p0 WHERE [UserId] = @p1", "InitialValue", 2);

			//Assert
			Assert.Equal(orig2.MenuVersion, edited2.MenuVersion);
			Assert.Equal(orig2.WorkDetectorRulesVersion, edited2.WorkDetectorRulesVersion);
			Assert.Equal(orig2.CensorRulesVersion, edited2.CensorRulesVersion);
			Assert.Equal(orig2.CollectorRulesVersion, edited2.CollectorRulesVersion);
			Assert.Equal(orig2.ClientSettingsVersion, edited2.ClientSettingsVersion);
		}

		[Fact]
		public void ClientSettingsVersionIncrementedOnAnyClientSettingUpdateFromNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1 };
			InsertThenUpdate(orig, null);
			var lastVersion = orig.ClientSettingsVersion;
			foreach (var update in clientSettingsUpdates)
			{
				var edited = Update(n => n.UserId == orig.UserId, update);

				//Assert
				Assert.Equal(orig.MenuVersion, edited.MenuVersion);
				Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
				Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
				Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
				Assert.True(lastVersion.ToLong() < edited.ClientSettingsVersion.ToLong());

				lastVersion = edited.ClientSettingsVersion;
			}
		}

		[Fact]
		public void ClientSettingsVersionIncrementedOnAnyClientSettingUpdate()
		{
			//Arrange
			var orig = GetClientSettingWithInitialValues();
			InsertThenUpdate(orig, null);
			var lastVersion = orig.ClientSettingsVersion;
			foreach (var update in clientSettingsUpdates)
			{
				var edited = Update(n => n.UserId == orig.UserId, update);

				//Assert
				Assert.Equal(orig.MenuVersion, edited.MenuVersion);
				Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
				Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
				Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
				Assert.True(lastVersion.ToLong() < edited.ClientSettingsVersion.ToLong());

				lastVersion = edited.ClientSettingsVersion;
			}
		}

		[Fact]
		public void ClientSettingsVersionIncrementedOnAnyClientSettingUpdateToNull()
		{
			//Arrange
			var orig = GetClientSettingWithInitialValues();
			InsertThenUpdate(orig, null);
			var lastVersion = orig.ClientSettingsVersion;
			foreach (var update in clientSettingsUpdatesToNull)
			{
				var edited = Update(n => n.UserId == orig.UserId, update);

				//Assert
				Assert.Equal(orig.MenuVersion, edited.MenuVersion);
				Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
				Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
				Assert.Equal(orig.CollectorRulesVersion, edited.CollectorRulesVersion);
				Assert.True(lastVersion.ToLong() < edited.ClientSettingsVersion.ToLong());

				lastVersion = edited.ClientSettingsVersion;
			}
		}

		[Fact]
		public void ClientSettingsVersionNotIncrementedOnAnyClientSettingUpdateToSameValue()
		{
			//Arrange
			var orig1 = new ClientSetting() { UserId = 1 };
			InsertThenUpdate(orig1, null);
			var lastVersion = orig1.ClientSettingsVersion;
			foreach (var columnName in clientSettingsUpdatesToInitialValues.Keys)
			{
				var edited1 = ExecuteCommand(n => n.UserId == orig1.UserId, "UPDATE [dbo].[ClientSettings] SET [" + columnName + "] = NULL WHERE [UserId] = @p0", 1);	//Can't pass in null as parameter. (null, (string)null, default(string), DBNull.Value)

				//Assert
				Assert.Equal(orig1.MenuVersion, edited1.MenuVersion);
				Assert.Equal(orig1.WorkDetectorRulesVersion, edited1.WorkDetectorRulesVersion);
				Assert.Equal(orig1.CensorRulesVersion, edited1.CensorRulesVersion);
				Assert.Equal(orig1.CollectorRulesVersion, edited1.CollectorRulesVersion);
				Assert.Equal(lastVersion, edited1.ClientSettingsVersion);

				lastVersion = edited1.ClientSettingsVersion;
			}

			//Arrange
			var orig2 = GetClientSettingWithInitialValues();
			orig2.UserId = 2;
			InsertThenUpdate(orig2, null);
			lastVersion = orig2.ClientSettingsVersion;
			foreach (var kv in clientSettingsUpdatesToInitialValues)
			{
				var edited2 = ExecuteCommand(n => n.UserId == orig2.UserId, "UPDATE [dbo].[ClientSettings] SET [" + kv.Key + "] = @p0 WHERE [UserId] = @p1", kv.Value, 2);

				//Assert
				Assert.Equal(orig2.MenuVersion, edited2.MenuVersion);
				Assert.Equal(orig2.WorkDetectorRulesVersion, edited2.WorkDetectorRulesVersion);
				Assert.Equal(orig2.CensorRulesVersion, edited2.CensorRulesVersion);
				Assert.Equal(orig2.CollectorRulesVersion, edited2.CollectorRulesVersion);
				Assert.Equal(lastVersion, edited2.ClientSettingsVersion);

				lastVersion = edited2.ClientSettingsVersion;
			}
		}

		[Fact]
		public void CollectorRulesVersionIncrementedOnCollectorRulesUpdateFromNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1 };
			var edited = InsertThenUpdate(orig, n => n.CollectorRules = "Edited");

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
			Assert.True(orig.CollectorRulesVersion.ToLong() < edited.CollectorRulesVersion.ToLong());
		}

		[Fact]
		public void CollectorRulesVersionIncrementedOnCollectorRulesUpdate()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, CollectorRules = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.CollectorRules = "Edited");

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
			Assert.True(orig.CollectorRulesVersion.ToLong() < edited.CollectorRulesVersion.ToLong());
		}

		[Fact]
		public void CollectorRulesVersionIncrementedOnCollectorRulesUpdateToNull()
		{
			//Arrange
			var orig = new ClientSetting() { UserId = 1, CollectorRules = "InitialValue" };
			var edited = InsertThenUpdate(orig, n => n.CollectorRules = null);

			//Assert
			Assert.Equal(orig.MenuVersion, edited.MenuVersion);
			Assert.Equal(orig.WorkDetectorRulesVersion, edited.WorkDetectorRulesVersion);
			Assert.Equal(orig.CensorRulesVersion, edited.CensorRulesVersion);
			Assert.Equal(orig.ClientSettingsVersion, edited.ClientSettingsVersion);
			Assert.True(orig.CollectorRulesVersion.ToLong() < edited.CollectorRulesVersion.ToLong());
		}

		[Fact]
		public void CollectorRulesVersionNotIncrementedOnCollectorRulesUpdateToSameValue()
		{
			//Arrange	//Update Menu from null to null
			var orig1 = new ClientSetting() { UserId = 1 };
			var edited1 = InsertThenExecuteCommand(orig1, "UPDATE [dbo].[ClientSettings] SET [CollectorRules] = NULL WHERE [UserId] = @p0", 1);	//Can't pass in null as parameter. (null, (string)null, default(string), DBNull.Value)


			//Assert
			Assert.Equal(orig1.MenuVersion, edited1.MenuVersion);
			Assert.Equal(orig1.WorkDetectorRulesVersion, edited1.WorkDetectorRulesVersion);
			Assert.Equal(orig1.CensorRulesVersion, edited1.CensorRulesVersion);
			Assert.Equal(orig1.CollectorRulesVersion, edited1.CollectorRulesVersion);
			Assert.Equal(orig1.ClientSettingsVersion, edited1.ClientSettingsVersion);

			//Arrange	//Update Menu from non null value to same non null value
			var orig2 = new ClientSetting() { UserId = 2, CollectorRules = "InitialValue" };
			var edited2 = InsertThenExecuteCommand(orig2, "UPDATE [dbo].[ClientSettings] SET [CollectorRules] = @p0 WHERE [UserId] = @p1", "InitialValue", 2);

			//Assert
			Assert.Equal(orig2.MenuVersion, edited2.MenuVersion);
			Assert.Equal(orig2.WorkDetectorRulesVersion, edited2.WorkDetectorRulesVersion);
			Assert.Equal(orig2.CensorRulesVersion, edited2.CensorRulesVersion);
			Assert.Equal(orig2.CollectorRulesVersion, edited2.CollectorRulesVersion);
			Assert.Equal(orig2.ClientSettingsVersion, edited2.ClientSettingsVersion);
		}

		private static ClientSetting GetClientSettingWithInitialValues()
		{
			return new ClientSetting()
			{
				UserId = 1,
				MenuUpdateInterval = 1,
				CaptureWorkItemInterval = 1,
				CaptureActiveWindowInterval = 1,
				CaptureScreenShotInterval = 1,
				TimeSyncThreshold = 1,
				JpegQuality = 1,
				JpegScalePct = 1,
				WorkTimeStartInMins = 1,
				WorkTimeEndInMins = 1,
				AfterWorkTimeIdleInMins = 1,
				MaxOfflineWorkItems = 1,
				DuringWorkTimeIdleInMins = 1,
				DuringWorkTimeIdleManualInterval = 1,
				MaxManualMeetingInterval = 1,
				RuleRestrictions = 1,
				IsMeetingTrackingEnabled = false,
				IsMeetingSubjectMandatory = false,
				BusyTimeThreshold = 1,
				CoincidentalClientsEnabled = false,
				IsManualMeetingStartsOnLock = false,
				IsLotusNotesMeetingTrackingEnabled = false,
				RuleMatchingInterval = 1,
			};
		}

		private readonly List<Action<ClientSetting>> clientSettingsUpdates = new List<Action<ClientSetting>>()
			{
				n => n.MenuUpdateInterval = 10,
				n => n.CaptureWorkItemInterval = 10,
				n => n.CaptureActiveWindowInterval = 10,
				n => n.CaptureScreenShotInterval = 10,
				n => n.TimeSyncThreshold = 10,
				n => n.JpegQuality = 10,
				n => n.JpegScalePct = 10,
				n => n.WorkTimeStartInMins = 10,
				n => n.WorkTimeEndInMins = 10,
				n => n.AfterWorkTimeIdleInMins = 10,
				n => n.MaxOfflineWorkItems = 10,
				n => n.DuringWorkTimeIdleInMins = 10,
				n => n.DuringWorkTimeIdleManualInterval = 10,
				n => n.MaxManualMeetingInterval = 10,
				n => n.RuleRestrictions = 10,
				n => n.IsMeetingTrackingEnabled = true,
				n => n.IsMeetingSubjectMandatory = true,
				n => n.BusyTimeThreshold = 10,
				n => n.CoincidentalClientsEnabled = true,
				n => n.IsManualMeetingStartsOnLock = true,
				n => n.IsLotusNotesMeetingTrackingEnabled = true,
				n => n.RuleMatchingInterval = 10,
			};

		private readonly List<Action<ClientSetting>> clientSettingsUpdatesToNull = new List<Action<ClientSetting>>()
			{
				n => n.MenuUpdateInterval = null,
				n => n.CaptureWorkItemInterval = null,
				n => n.CaptureActiveWindowInterval = null,
				n => n.CaptureScreenShotInterval = null,
				n => n.TimeSyncThreshold = null,
				n => n.JpegQuality = null,
				n => n.JpegScalePct = null,
				n => n.WorkTimeStartInMins = null,
				n => n.WorkTimeEndInMins = null,
				n => n.AfterWorkTimeIdleInMins = null,
				n => n.MaxOfflineWorkItems = null,
				n => n.DuringWorkTimeIdleInMins = null,
				n => n.DuringWorkTimeIdleManualInterval = null,
				n => n.MaxManualMeetingInterval = null,
				n => n.RuleRestrictions = null,
				n => n.IsMeetingTrackingEnabled = null,
				n => n.IsMeetingSubjectMandatory = null,
				n => n.BusyTimeThreshold = null,
				n => n.CoincidentalClientsEnabled = null,
				n => n.IsManualMeetingStartsOnLock = null,
				n => n.IsLotusNotesMeetingTrackingEnabled = null,
				n => n.RuleMatchingInterval = null,
			};

		private readonly Dictionary<string, object> clientSettingsUpdatesToInitialValues = new Dictionary<string, object>()
			{
				{ "MenuUpdateInterval", 1},
				{ "CaptureWorkItemInterval", 1},
				{ "CaptureActiveWindowInterval", 1},
				{ "CaptureScreenShotInterval", 1},
				{ "TimeSyncThreshold", 1},
				{ "JpegQuality", 1},
				{ "JpegScalePct", 1},
				{ "WorkTimeStartInMins", 1},
				{ "WorkTimeEndInMins", 1},
				{ "AfterWorkTimeIdleInMins", 1},
				{ "MaxOfflineWorkItems", 1},
				{ "DuringWorkTimeIdleInMins", 1},
				{ "DuringWorkTimeIdleManualInterval", 1},
				{ "MaxManualMeetingInterval", 1},
				{ "RuleRestrictions", 1},
				{ "IsMeetingTrackingEnabled", false},
				{ "IsMeetingSubjectMandatory", false},
				{ "BusyTimeThreshold", 1},
				{ "CoincidentalClientsEnabled", false},
				{ "IsManualMeetingStartsOnLock", false},
				{ "IsLotusNotesMeetingTrackingEnabled", false},
				{ "RuleMatchingInterval", 1},
			};

		private static ClientSetting InsertThenUpdate(ClientSetting orig, Action<ClientSetting> update)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(orig);
				context.SubmitChanges();
			}

			return update == null ? orig : Update(n => n.UserId == orig.UserId, update);
		}

		private static ClientSetting Update(Func<ClientSetting, bool> predicate, Action<ClientSetting> update)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.Log = Console.Out;
				var edited = context.ClientSettings.Single(predicate);
				update(edited);
				context.SubmitChanges();
				return edited;
			}
		}

		private static ClientSetting InsertThenExecuteCommand(ClientSetting orig, string sql, params object[] parameters)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(orig);
				context.SubmitChanges();
			}

			if (sql == null) return orig;

			return ExecuteCommand(n => n.UserId == orig.UserId, sql, parameters);
		}

		private static ClientSetting ExecuteCommand(Func<ClientSetting, bool> predicate, string sql, params object[] parameters)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.Log = Console.Out;
				context.ExecuteCommand(sql, parameters);
				var edited = context.ClientSettings.Single(predicate);
				return edited;
			}
		}

		[Fact]
		public void AdoNetClientSettingsReadsAreTheSameForMenu()
		{
			//Arrange
			var newMenu = new ClientMenu()
			{
				Works = new List<WorkData>()
			    {
			        new WorkData("őőúűáóüöíé", 23, 1),
			        new WorkData("X", null, null)
			            {
			                Children = new List<WorkData>()
			                            {
			                                new WorkData("XY", null, null)
			                                    {
			                                        Children = new List<WorkData>()
			                                                    {
			                                                        new WorkData("XYX2", 2, 0) { CategoryId = 2 },
			                                                        new WorkData("XYY4", 4, 0) { CategoryId = 1 },
			                                                        new WorkData("XYZ3", 3, 0),
			                                                    }
			                                    },
			                                new WorkData("", null, 0),
			                                new WorkData("XZ1", 1, null),
			                            }
			            },
			        new WorkData("devenv.exe", null, null)
			            {
			                ProjectId = 2,
			                Children = new List<WorkData>()
			                    {
			                        new WorkData("40", 40, null),
			                        new WorkData("41", 41, null),
			                        new WorkData("42", 42, null),
			                    }
			            },
			        new WorkData("firefox.exe", null, null)
			            {
			                ProjectId = 3,
			                Children = new List<WorkData>()
			                    {
			                        new WorkData("50", 50, null),
			                        new WorkData("51", 51, null),
			                        new WorkData("52", 52, null),
			                    }
			            },
			    },
				CategoriesById = new Dictionary<int, CategoryData>() { 
			        {1, new CategoryData() { Id = 1, Name = "Gmail -"}},
			        {2, new CategoryData() { Id = 2, Name = "Total"}},
			    },
			};
			string menuData;
			using (var stream = new MemoryStream())
			{
				XmlPersistenceHelper.WriteToStream(stream, newMenu);
				menuData = Encoding.UTF8.GetString(stream.ToArray());
			}
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
					Menu = menuData
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var linq = context.ClientSettings.Where(n => n.UserId == 13).Single();
				var adonet = context.GetClientMenu(13);
				string dbMenuData;
				using (var stream = new MemoryStream())
				{
					XmlPersistenceHelper.WriteToStream(stream, adonet.Value);
					dbMenuData = Encoding.UTF8.GetString(stream.ToArray());
				}

				//Assert
				Assert.Equal(linq.Menu, dbMenuData);
				Assert.Equal(linq.MenuVersion.ToString(), adonet.Version);
			}
		}

		[Fact]
		public void CanUseAdoNetAsFirst()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var adonet = context.GetClientMenu(13);
				//Assert
				Assert.NotNull(adonet);
				Assert.NotNull(adonet.Version);
				Assert.Null(adonet.Value);
			}
		}


		[Fact]
		public void CanUseAdoNetAfterLinq()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var linq = context.ClientSettings.Where(n => n.UserId == 13).Single();
				var adonet = context.GetClientMenu(13);
				//Assert
				Assert.NotNull(linq);
				Assert.NotNull(adonet);
				Assert.NotNull(adonet.Version);
				Assert.Null(adonet.Value);
			}
		}

		[Fact]
		public void CanUseAdoNetAfterLinqAndThenLinq()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
				});
				context.SubmitChanges();
			}

			//Act
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var linq = context.ClientSettings.Where(n => n.UserId == 13).Single();
				var adonet = context.GetClientMenu(13);
				linq = context.ClientSettings.Where(n => n.UserId == 13).Single();
				//Assert
				Assert.NotNull(linq);
				Assert.NotNull(adonet);
				Assert.NotNull(adonet.Version);
				Assert.Null(adonet.Value);
			}
		}

		[Fact]
		public void GetWorkDetectorRules()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
				});
				context.SubmitChanges();
			}

			//Act/Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.GetWorkDetectorRules(13);
			}
		}

		[Fact]
		public void GetCensorRules()
		{
			//Arrange
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ClientSettings.InsertOnSubmit(new ClientSetting()
				{
					UserId = 13,
				});
				context.SubmitChanges();
			}

			//Act/Assert
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.GetCensorRules(13);
			}
		}
	}
}
