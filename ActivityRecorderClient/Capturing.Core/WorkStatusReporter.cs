using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using log4net;

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

		//it is not trivial how to avoid stack dive so that is why this is a bit complicated
		private void SendStatusDataAsync(WorkStatusChange dataParam)
		{
			if (dataParam == null) return;
			var data = dataParam;
			while (data != null)
			{
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
				//in practice BeginXXX can also throw... so we definitely want to catch it...
				try
				{
					if (aState.Data.WorkId == null)
					{
						var ar = aState.Client.Client.BeginStopWork(ConfigManager.UserId, ConfigManager.EnvironmentInfo.ComputerId, aState.Data.CreateDate, aState.SendDate, StopWorkCallback, aState);
						if (!ar.CompletedSynchronously) return;
						data = EndStopWork(ar);
					}
					else
					{
						var ar = aState.Client.Client.BeginStartWork(ConfigManager.UserId, aState.Data.WorkId.Value, ConfigManager.EnvironmentInfo.ComputerId, aState.Data.CreateDate, aState.SendDate, StartWorkCallback, aState);
						if (!ar.CompletedSynchronously) return;
						data = EndStartWork(ar);
					}
				}
				catch (Exception ex)
				{
					aState.Client.CloseIfUnusable(ex);
					aState.Client.Dispose();
					log.Error("Error in BeginXXX", ex);
					Thread.Sleep(1000);
				}
			}
		}

		private void StopWorkCallback(IAsyncResult ar)
		{
			if (ar.CompletedSynchronously) return;
			var newData = EndStopWork(ar);
			SendStatusDataAsync(newData);
		}

		private void StartWorkCallback(IAsyncResult ar)
		{
			if (ar.CompletedSynchronously) return;
			var newData = EndStartWork(ar);
			SendStatusDataAsync(newData);
		}

		// ReSharper disable EmptyGeneralCatchClause
		private WorkStatusChange EndStartWork(IAsyncResult ar)
		{
			var aState = (AsyncData)ar.AsyncState;
			var success = false;
			try
			{
				aState.Client.Client.EndStartWork(ar);
				success = true;
			}
			catch
			{
			}
			finally
			{
				aState.Client.Dispose();
				if (!success) Thread.Sleep(1000);
			}
			return GetDataToSend(success ? aState : null);
		}

		private WorkStatusChange EndStopWork(IAsyncResult ar)
		{
			var aState = (AsyncData)ar.AsyncState;
			var success = false;
			try
			{
				aState.Client.Client.EndStopWork(ar);
				success = true;
			}
			catch
			{
			}
			finally
			{
				aState.Client.Dispose();
				if (!success) Thread.Sleep(1000);
			}
			return GetDataToSend(success ? aState : null);
		}
		// ReSharper restore EmptyGeneralCatchClause

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
