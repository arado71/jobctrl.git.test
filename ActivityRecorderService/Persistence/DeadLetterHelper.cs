using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.Meeting;

namespace Tct.ActivityRecorderService.Persistence
{
	public static class DeadLetterHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly DeadLetterPathResolver pathResolver;

		internal static Func<string, Stream> FileWriteStreamFactory
		{
			get { return XmlPersistenceHelper.FileWriteStreamFactory; }
			set { XmlPersistenceHelper.FileWriteStreamFactory = value; }
		}

		internal static Func<string, Stream> FileReadStreamFactory
		{
			get { return XmlPersistenceHelper.FileReadStreamFactory; }
			set { XmlPersistenceHelper.FileReadStreamFactory = value; }
		}

		static DeadLetterHelper()
		{
			var dir = ConfigManager.DeadLetterDir;
			try
			{
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to create dir " + dir, ex);
			}
			pathResolver = new DeadLetterPathResolver(dir);
		}

		public static bool TrySaveItem(WorkItem workItem, Exception exception)
		{
			var item = new DeadLetterItem()
			{
				ItemType = "WorkItem",
				UserId = workItem.UserId,
				WorkId = workItem.WorkId,
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate,
				ErrorText = exception.Message,
			};
			return TrySaveDeadLetterItem(item, workItem);
		}

		public static bool TrySaveItem(ManualWorkItem workItem, Exception exception)
		{
			var item = new DeadLetterItem()
			{
				ItemType = "ManualWorkItem",
				UserId = workItem.UserId,
				WorkId = workItem.WorkId,
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate,
				ErrorText = exception.Message,
			};
			return TrySaveDeadLetterItem(item, workItem);
		}

		public static bool TrySaveItem(ManualMeetingDataDead workItem, Exception exception)
		{
			var item = new DeadLetterItem()
			{
				ItemType = "ManualMeetingDataDead",
				UserId = workItem.UserId,
				WorkId = workItem.ManualMeetingData.WorkId,
				StartDate = workItem.ManualMeetingData.StartTime,
				EndDate = workItem.ManualMeetingData.EndTime,
				ErrorText = exception.Message,
			};
			return TrySaveDeadLetterItem(item, workItem);
		}

		public static bool TrySaveItem(ParallelWorkItem workItem, Exception exception)
		{
			var item = new DeadLetterItem()
			{
				ItemType = "ParallelWorkItem",
				UserId = workItem.UserId,
				WorkId = workItem.WorkId,
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate,
				ErrorText = exception.Message,
			};
			return TrySaveDeadLetterItem(item, workItem);
		}

		public static bool TrySaveItem(FinishedMeetingEntryDead workItem, Exception exception)
		{
			var item = new DeadLetterItem()
			{
				ItemType = "FinishedMeetingEntryDead",
				UserId = workItem.UserId,
				WorkId = null,
				StartDate = workItem.FinishedMeetingEntry.StartTime,
				EndDate = workItem.FinishedMeetingEntry.EndTime,
				ErrorText = exception.Message,
			};
			return TrySaveDeadLetterItem(item, workItem);
		}

		private static readonly DateTime minDate = new DateTime(1753, 1, 1);
		private static bool TrySaveDeadLetterItem<T>(DeadLetterItem item, T workItem)
		{
			try
			{
				if (item.StartDate < minDate || item.EndDate < minDate)
				{
					log.Error("Invalid date in dead letter item " + item);
					if (item.StartDate < minDate) item.StartDate = minDate;
					if (item.EndDate < minDate) item.EndDate = minDate;
				}
				using (var context = new AggregateDataClassesDataContext())
				{
					context.Connection.Open();
					context.Connection.SetXactAbortOn();
					using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
					{
						context.DeadLetterItems.InsertOnSubmit(item);
						context.SubmitChanges();
						var path = Path.Combine(pathResolver.GetRootDir(), pathResolver.GetFilePath(item));
						XmlPersistenceHelper.SaveToFile(path, workItem);
						context.Transaction.Commit();
					}
					log.Debug("Created dead letter item " + item);
					return true;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to save " + item.ItemType + " " + workItem, ex);
				return false;
			}
		}

		public static bool TryLoadItem<T>(string path, out T workItem)
		{
			try
			{
				XmlPersistenceHelper.LoadFromFile(path, out workItem);
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Unable to load item from path " + path, ex);
				workItem = default(T);
				return false;
			}
		}
	}
}
