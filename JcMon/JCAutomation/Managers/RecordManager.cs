using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using JCAutomation.Data;
using log4net;

namespace JCAutomation.Managers
{
	public class RecordManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private ControlInfo lastControl = null;

		private List<CaptureInfo> captures = new List<CaptureInfo>();

		public RecordManager()
			: base(log)
		{
		}

		public void AddCapture(ControlInfo controlInfo)
		{
			lock (captures)
			{
				if (!controlInfo.Equals(lastControl))
				{
					captures.Add(new CaptureInfo(controlInfo, "") {Type = CaptureType.Automatic});
					lastControl = controlInfo;
				}
			}
		}

		public void Save(bool inBackground = true)
		{
			ThreadPool.QueueUserWorkItem(_ => SaveCaptures(), null);
		}

		public override void Stop()
		{
			SaveCaptures();
			base.Stop();
		}

		protected override void ManagerCallbackImpl()
		{
			SaveCaptures();
		}

		private void SaveCaptures()
		{
			try
			{
				List<CaptureInfo> controlsToSave;
				lock (captures)
				{
					controlsToSave = captures;
					captures = new List<CaptureInfo>();
				}

				SaveCaptures(controlsToSave);
			}
			catch (Exception ex)
			{
				log.Error("Failed to save captures", ex);
			}
		}

		private void SaveCaptures(List<CaptureInfo> data)
		{
			try
			{
				var formatter = new BinaryFormatter();
				using (var fs = new FileStream(DateTime.UtcNow.ToString("yy-MM-dd_HH-mm-ss") + ".jch", FileMode.CreateNew))
				{
					formatter.Serialize(fs, data);
				}
			}
			catch (IOException ex)
			{
				int hr = Marshal.GetHRForException(ex);
				if (hr == 0x80070050)
				{
					Thread.Sleep(1000);
					SaveCaptures(data);
				}
				else
				{
					throw;
				}
			}
		}

		protected override int ManagerCallbackInterval
		{
			get
			{
				return Configuration.RecordInterval;
			}
		}
	}
}
