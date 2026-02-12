using log4net;
using System;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public class SnippingDataUploadManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#if DEBUG || DEV
		private const int callbackInterval =  2 * 60 * 1000;  // 2 minutes
#else
		private const int callbackInterval = 60 * 60 * 1000;  // 1 hour
#endif
		public SnippingDataUploadManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get { return callbackInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				ImageStorageCleaner.CleanStoredImages(true);
				log.Debug("OCR uploading contented items");
				foreach (Snippet item in IsolatedStorageHelper.ContentedItems)
				{
					if (ActivityRecorderClientWrapper.Execute(n => n.AddSnippet(item)))
						IsolatedStorageHelper.Delete(item.Guid);
				}
			}
			catch (Exception e)
			{
				log.Error(e.Message);
			}
		}
	}
}
