using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderService.ClientComputerData;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ClientComputerErrorsTests : DbTestsBase
	{
		private readonly DateTime now = new DateTime(2014, 06, 01, 15, 00, 00);
		private readonly Guid guid = new Guid("bdbbac62-ea22-4c1b-b132-6969a1a02b4a");

		private ClientComputerError GetClientComputerError()
		{
			return new ClientComputerError()
			{
				UserId = 13,
				ComputerId = 1,
				ClientId = guid,
				Major = 2,
				Minor = 2,
				Build = 1,
				Revision = 0,
				Description = "valami",
				Features = "Alma, Körte",
				HasAttachment = true,
				Data = new byte[] { 1 },
				Offset = 0,
				IsCompleted = false,
			};
		}

		[Fact]
		public void CanInsertOne()
		{
			//Arrange
			var data = GetClientComputerError();
			data.Data = null;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				//Act
				context.SubmitChanges();
			}
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CanInsertOneWithData()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				//Act
				context.SubmitChanges();
			}
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(1, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CanInsertOneWithUpsert()
		{
			//Arrange
			var data = GetClientComputerError();
			Assert.Equal(0, data.Offset);
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				//Act
				Assert.Equal(1, context.UpsertClientComputerError(data));
			}
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(1, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CannotInsertOneWithUpsertWrongOffset()
		{
			//Arrange
			var data = GetClientComputerError();
			data.Offset = 187;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				//Assert
				Assert.Throws<SqlException>(() => context.UpsertClientComputerError(data));
			}
		}

		[Fact]
		public void CanUpdateOnce()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(1, context.UpsertClientComputerError(data));
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(2, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CanUpdateOnceWithIsCompletedSet()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 1;
				data.IsCompleted = true;
				Assert.Equal(1, context.UpsertClientComputerError(data));
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(2, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CanUpdateTwice()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(1, context.UpsertClientComputerError(data));
			}

			//Act
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 2;
				Assert.Equal(1, context.UpsertClientComputerError(data));
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(3, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CannotUpdateAfterIsCompletedSet()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				data.IsCompleted = true;
				Assert.Equal(1, context.UpsertClientComputerError(data));
			}

			//Act
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 2;
				//Assert
				Assert.Throws<SqlException>(() => context.UpsertClientComputerError(data));
			}
		}

		[Fact]
		public void CannotUpdateAfterEndDateSetInTran()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				data.IsCompleted = true;
				Assert.Equal(1, context.UpsertClientComputerError(data));
			}

			//Act
			using (var context = new ClientComputerInfoDataClassesDataContext())
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
							context.UpsertClientComputerError(data);
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
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 1;
				data.IsCompleted = true;
				Assert.Equal(1, context.UpsertClientComputerError(data));
			}

			//Act
			ClientComputerError dataDb;
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				data.Id = 0;
				data.Offset = 1;
				Assert.Equal(0, context.UpsertClientComputerError(data));
				dataDb = context.ClientComputerErrors.Single();
			}

			//Assert
			Assert.Equal(dataDb.Id, data.Id);
			Assert.True(dataDb.FirstReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > now);
			Assert.True(dataDb.LastReceiveDate > dataDb.FirstReceiveDate);
			Assert.Equal(data.UserId, dataDb.UserId);
			Assert.Equal(data.ComputerId, dataDb.ComputerId);
			Assert.Equal(data.ClientId, dataDb.ClientId);
			Assert.Equal(data.Major, dataDb.Major);
			Assert.Equal(data.Minor, dataDb.Minor);
			Assert.Equal(data.Build, dataDb.Build);
			Assert.Equal(data.Revision, dataDb.Revision);
			Assert.Equal(data.Description, dataDb.Description);
			Assert.Equal(data.Features, dataDb.Features);
			Assert.Equal(data.HasAttachment, dataDb.HasAttachment);
			Assert.Equal(2, dataDb.Offset);
			//Assert.Equal(data.Offset, dataDb.Offset);
			Assert.Equal(data.IsCompleted, dataDb.IsCompleted);
		}

		[Fact]
		public void CannotUpdateWrongOffset()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}

			//Act
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Thread.Sleep(100);
				data.Id = 0;
				data.Offset = 2;
				//Assert
				Assert.Throws<SqlException>(() => context.UpsertClientComputerError(data));
			}
		}

		[Fact]
		public void CannotInsertSameGuid()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				context.SubmitChanges();
			}
			data = new ClientComputerError() { ClientId = data.ClientId };
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ClientComputerErrors.InsertOnSubmit(data);
				Assert.Throws<SqlException>(() =>
				{
					try
					{
						context.SubmitChanges();
					}
					catch (SqlException ex)
					{
						//Console.WriteLine(ex); 
						Assert.True(ex.Message.Contains("IX_ClientComputerErrors_ClientId"));
						throw;
					}
				});
			}
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Assert.Equal(1, context.ClientComputerErrors.Count());
			}
		}

		[Fact]
		public void CannotInsertSameGuidWithUpsertInTran()
		{
			//Arrange
			var data = GetClientComputerError();
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.UpsertClientComputerError(data);
			}
			data = new ClientComputerError() { ClientId = data.ClientId };
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.Connection.Open();
				using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
				{
					context.ClientComputerErrors.InsertOnSubmit(data);
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
							Assert.True(ex.Message.Contains("IX_ClientComputerErrors_ClientId"));
							throw;
						}
					});
				}
			}
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				Assert.Equal(1, context.ClientComputerErrors.Count());
			}
		}

	}
}
