using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using log4net;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Sends Online/Offline statuses to the server
	/// </summary>
	//todo handle when upload is disabled
	public class WorkStatusReporter : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly CaptureManager captureManager;
		private readonly object thisLock = new object(); //lock for dataToSend, isDisposed
		private WorkStatusChange dataToSend;
		private bool isDisposed;

		public WorkStatusReporter(CaptureManager captureManager)
		{
			this.captureManager = captureManager;
			this.captureManager.WorkStatusChanged += CaptureManagerWorkStatusChanged; //this is raised on the gui thread
		}

		private void CaptureManagerWorkStatusChanged(object sender, SingleValueEventArgs<WorkStatusChange> e)
		{
			Debug.Assert(e.Value != null);
			var shouldStart = SetDataToSend(e.Value);
			if (!shouldStart) return;
			ThreadPool.QueueUserWorkItem(_ => SendStatusDataAsync(e.Value)); //on error we have a Thread.Sleep and we don't want that on the GUI (it would be better to use a timer, but that would complicate things even more)
		}

		private bool SetDataToSend(WorkStatusChange data)
		{
			lock (thisLock)
			{
				if (isDisposed) return false;
				var shouldStart = dataToSend == null;
				dataToSend = data;
				return shouldStart;
			}
		}

		private WorkStatusChange GetDataToSend(AsyncData sentData)
		{
			lock (thisLock)
			{
				if (isDisposed) return null;
				if (sentData != null
					&& sentData.Data == dataToSend
					&& DateTime.UtcNow - sentData.SendDate < TimeSpan.FromSeconds(10)) //10 secs might be too small?
				{
					dataToSend = null; //we've sent this data on time
					return null;
				}
				return dataToSend;
			}
		}

		private async Task SendStatusDataAsync(WorkStatusChange dataParam)
		{
			if (dataParam == null) return;
			var data = dataParam;
			while (data != null)
			{
                lock (thisLock)
                {
                    if (isDisposed) return;
                }

                AsyncData aState;
				try
				{
					//use async client but only one call will be active at a time
					aState = new AsyncData() { Client = new ActivityRecorderClientWrapper(), Data = data, SendDate = DateTime.UtcNow };
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception ex) //this should only happen if the saved wcf endpoint is no longer in the config
				{
					lock (thisLock)
					{
						if (isDisposed) return;
					}
					log.ErrorAndFail("Cannot create client", ex);
					Thread.Sleep(1000);
					break;
				}

				var success = true;
				try
				{
					Task result;
					if (aState.Data.WorkId == null)
					{
						await aState.Client.Client.StopWorkAsync(ConfigManager.UserId, ConfigManager.EnvironmentInfo.ComputerId, aState.Data.CreateDate, aState.SendDate).ConfigureAwait(false);
					}
					else
					{
						await aState.Client.Client.StartWorkAsync(ConfigManager.UserId, aState.Data.WorkId.Value, ConfigManager.EnvironmentInfo.ComputerId, aState.Data.CreateDate, aState.SendDate);
					}
				}
				catch (Exception ex)
				{
					success = false;
					aState.Client.CloseIfUnusable(ex);
					Thread.Sleep(1000);
				}
				finally
				{
					data = GetDataToSend(success ? aState : null);
                    aState.Client.Dispose();
                }
            }
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				if (isDisposed) return;
				isDisposed = true;
			}
			this.captureManager.WorkStatusChanged -= CaptureManagerWorkStatusChanged;
		}

		private class AsyncData
		{
			public ActivityRecorderClientWrapper Client { get; set; }
			public WorkStatusChange Data { get; set; }
			public DateTime SendDate { get; set; }
		}
	}
}
