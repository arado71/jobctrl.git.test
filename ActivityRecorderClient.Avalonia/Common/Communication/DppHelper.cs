using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Communication
{
	/// <summary>
	/// Data Protection Policy (DPP) querying information and recording acceptance
	/// </summary>
	public static class DppHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static string DppDataPath => "DppData-" + ConfigManager.UserId;

		public static AcceptanceData GetAcceptanceData()
		{
			try
			{
				var data = ActivityRecorderClientWrapper.Execute(n => n.GetDppInformation(ConfigManager.UserId));
				if (data != null)
				{
					IsolatedStorageSerializationHelper.Save(DppDataPath, data);
					log.DebugFormat("Dpp data retrieved and saved (AcceptedAt: {0}, Msg: {1}" , data.AcceptedAt, (data.Message.Length > 15 ? data.Message.Substring(0, 15) + "..." : data.Message));
				}
				else
				{
					if (IsolatedStorageSerializationHelper.Exists(DppDataPath))
					{
						IsolatedStorageSerializationHelper.Delete(DppDataPath);
						log.Debug("Saved dpp data deleted");
					}
				}

				return data;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("GetAcceptanceMessage", log, ex);
				if (IsolatedStorageSerializationHelper.Exists(DppDataPath))
				{
					try
					{
						IsolatedStorageSerializationHelper.Load(DppDataPath, out AcceptanceData data);
						log.Debug("Saved dpp data loaded");
						return data;
					}
					catch (Exception ex2)
					{
						log.Warn("Saved dpp data can't be loaded", ex2);
						IsolatedStorageSerializationHelper.Load(DppDataPath, out string msg);
						log.Debug("Saved old dpp data loaded");
						return new AcceptanceData { Message = msg };
					}
				}

				return null;
			}
		}

		public static void SetAcceptanceDate(CaptureCoordinator captureCoordinator)
		{
			var item = new AcceptanceDateItem() { Id = Guid.NewGuid(), StartDate = DateTime.UtcNow };
			log.DebugFormat("Upload item created for recording acceptance date (UID:{0}, AcceptedAt:{1}", item.Id, item.StartDate.ToLongDateString());
			ThreadPool.QueueUserWorkItem(_ => captureCoordinator.WorkItemManager.SendOrPersist(item));
			if (IsolatedStorageSerializationHelper.Exists(DppDataPath))
			{
				IsolatedStorageSerializationHelper.Load(DppDataPath, out AcceptanceData data);
				data.AcceptedAt = item.StartDate;
				IsolatedStorageSerializationHelper.Save(DppDataPath, data);
				log.Debug("Saved dpp data updated after accepted");
			}
		}

	}

	[DataContract]
	[KnownType(typeof(AcceptanceDateItem))]
	public class AcceptanceDateItem : IUploadItem
	{
		[IgnoreDataMember]
		public int UserId => ConfigManager.UserId;

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public DateTime StartDate { get; set; }
	}
}
