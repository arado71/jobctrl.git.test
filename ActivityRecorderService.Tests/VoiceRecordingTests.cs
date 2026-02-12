using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderService.Voice;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class VoiceRecordingTests : DbTestsBase
	{
		private readonly DateTime now = new DateTime(2013, 05, 22, 15, 00, 00);
		private readonly Guid guid = new Guid("8d252f3c-f1a0-4c29-8148-cd0392a66572");

		private VoiceRecording GetVoiceRecording()
		{
			return new VoiceRecording()
			{
				UserId = 13,
				WorkId = 2,
				ClientId = guid,
				StartDate = now,
				Duration = 100,
				Codec = 1,
				Extension = "mp3",
				Offset = 0,
				Name = "valami",
				Data = new byte[] { 1 },
			};
		}

		#region Upsert

		[Fact]
		public void CanInsertOne()
		{
			//Arrange
			var data = GetVoiceRecording();
			data.Data = null;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				//Act
				context.SubmitChanges();
			}
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
			Assert.Equal(data.DeleteDate, dataDb.DeleteDate);
			Assert.Null(dataDb.DeleteDate);
		}

		[Fact]
		public void CanInsertOneWithData()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				//Act
				context.SubmitChanges();
			}
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(1, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
		}

		[Fact]
		public void CanInsertOneWithUpsert()
		{
			//Arrange
			var data = GetVoiceRecording();
			Assert.Equal(0, data.Offset);
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				//Act
				Assert.Equal(1, context.UpsertVoiceRecording(data));
			}
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(1, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
		}

		[Fact]
		public void CannotInsertOneWithUpsertWrongOffset()
		{
			//Arrange
			var data = GetVoiceRecording();
			data.Offset = 1;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				//Assert
				Assert.Throws<SqlException>(() => context.UpsertVoiceRecording(data));
			}
		}

		[Fact]
		public void CanUpdateOnce()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(1, context.UpsertVoiceRecording(data));
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(2, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
		}

		[Fact]
		public void CanUpdateOnceName()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Name = "ujnev";
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(1, context.UpsertVoiceRecording(data));
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(2, dataDb.Offset);
			Assert.Equal("ujnev", dataDb.Name);
		}

		[Fact]
		public void CanUpdateOnceWithEndDateSet()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 1;
				data.EndDate = now.AddMinutes(1);
				Assert.Equal(1, context.UpsertVoiceRecording(data));
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(2, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
		}

		[Fact]
		public void CanUpdateTwice()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(1, context.UpsertVoiceRecording(data));
			}

			//Act
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 2;
				Assert.Equal(1, context.UpsertVoiceRecording(data));
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(3, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
		}

		[Fact]
		public void CannotUpdateAfterEndDateSet()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				data.EndDate = now.AddMinutes(1);
				Assert.Equal(1, context.UpsertVoiceRecording(data));
			}

			//Act
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 2;
				//Assert
				Assert.Throws<SqlException>(() => context.UpsertVoiceRecording(data));
			}
		}

		[Fact]
		public void CannotUpdateAfterEndDateSetInTran()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				data.EndDate = now.AddMinutes(1);
				Assert.Equal(1, context.UpsertVoiceRecording(data));
			}

			//Act
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.Connection.Open();
				using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
				{
					data.Id = 0;
					data.Offset = 2;
					//Assert
					Assert.Throws<SqlException>(() =>
													{
														try
														{
															context.UpsertVoiceRecording(data);
															context.Transaction.Commit();
														}
														catch (SqlException ex)
														{
															Console.WriteLine(ex);
															//Assert.True(!ex.Message.Contains("Transaction count after EXECUTE indicates a mismatching number of BEGIN and COMMIT statements."));
															throw;
														}
													});
				}
			}
		}

		[Fact]
		public void CanIgnoreUpdateAfterEndDateSetWithSameOffset()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 1;
				data.EndDate = now.AddMinutes(1);
				Assert.Equal(1, context.UpsertVoiceRecording(data));
			}

			//Act
			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(0, context.UpsertVoiceRecording(data));
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(2, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
		}

		[Fact]
		public void CannotUpdateWrongOffset()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 2;
				//Assert
				Assert.Throws<SqlException>(() => context.UpsertVoiceRecording(data));
			}
		}

		[Fact]
		public void CannotInsertSameGuid()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}
			data = new VoiceRecording() { ClientId = data.ClientId, StartDate = data.StartDate.AddHours(1), };
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				Assert.Throws<SqlException>(() =>
												{
													try
													{
														context.SubmitChanges();
													}
													catch (SqlException ex)
													{
														//Console.WriteLine(ex); 
														Assert.True(ex.Message.Contains("IX_VoiceRecordings_ClientId"));
														throw;
													}
												});
			}
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Assert.Equal(1, context.VoiceRecordings.Count());
			}
		}

		[Fact]
		public void CannotInsertSameGuidWithUpsertInTran()
		{
			//Arrange
			var data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.UpsertVoiceRecording(data);
			}
			data = new VoiceRecording() { ClientId = data.ClientId, StartDate = data.StartDate.AddHours(1), };
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.Connection.Open();
				using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
				{
					context.VoiceRecordings.InsertOnSubmit(data);
					Assert.Throws<SqlException>(() =>
													{
														try
														{
															context.SubmitChanges();
														}
														catch (SqlException ex)
														{
															//Console.WriteLine(ex);
															Assert.True(!ex.Message.Contains("Transaction count after EXECUTE indicates a mismatching number of BEGIN and COMMIT statements."));
															Assert.True(ex.Message.Contains("IX_VoiceRecordings_ClientId"));
															throw;
														}
													});
				}
			}
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Assert.Equal(1, context.VoiceRecordings.Count());
			}
		}
		#endregion

		#region Delete

		[Fact]
		public void CanDeleteOne()
		{
			//Arrange
			var data = GetVoiceRecording();
			data.Data = null;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Assert.Equal(1, context.DeleteThisVoiceRecording(data));
			}

			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
			Assert.Equal(data.DeleteDate, dataDb.DeleteDate);
			Assert.NotNull(dataDb.DeleteDate);
		}

		[Fact]
		public void SecondDeleteIgnored()
		{
			//Arrange
			var data = GetVoiceRecording();
			data.Data = null;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Assert.Equal(1, context.DeleteThisVoiceRecording(data));
			}

			VoiceRecording dataDb;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				dataDb = context.VoiceRecordings.Single();
			}
			var origDelete = dataDb.DeleteDate.Value;
			Thread.Sleep(100);

			data = GetVoiceRecording();
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Assert.Equal(0, context.DeleteThisVoiceRecording(data));
				context.SubmitChanges();
			}

			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				dataDb = context.VoiceRecordings.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.WorkId, dataDb.WorkId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.StartDate, dataDb.StartDate);
			Assert.Equal(data.EndDate, dataDb.EndDate);
			Assert.Equal(data.Duration, dataDb.Duration);
			Assert.Equal(data.Codec, dataDb.Codec);
			Assert.Equal(data.Extension, dataDb.Extension);
			Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.Name, dataDb.Name);
			Assert.Equal(data.DeleteDate, dataDb.DeleteDate);
			Assert.Equal(origDelete, dataDb.DeleteDate);
		}

		[Fact]
		public void CannotDeleteOtherUsersData()
		{
			//Arrange
			var data = GetVoiceRecording();
			data.Data = null;
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				context.VoiceRecordings.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			data = GetVoiceRecording();
			data.UserId++; //change userid
			using (var context = new VoiceRecorderDataClassesDataContext())
			{
				Assert.Throws<SqlException>(() => context.DeleteThisVoiceRecording(data));
			}
		}
		#endregion
	}
}
