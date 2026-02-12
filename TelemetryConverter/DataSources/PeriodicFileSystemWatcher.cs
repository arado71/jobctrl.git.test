using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.DataSources
{
	class PeriodicFileSystemWatcher : PeriodicManager
	{

		private DateTime lastCheck = DateTime.MinValue;
		private readonly string remoteAddress;

		public event EventHandler<FileSystemEventArgs> DataAvailable;
		public PeriodicFileSystemWatcher(string remoteAddress):base(false)
		{
			this.remoteAddress = remoteAddress;
		}
		protected override void ManagerCallbackImpl()
		{
			foreach (var info in new DirectoryInfo(remoteAddress).GetFiles("*.*", SearchOption.AllDirectories))
			{
				if (info.CreationTime > lastCheck)
				{
					OnDataAvailable(info.FullName);
				}
			}
			lastCheck = DateTime.Now;
		}
		private void OnDataAvailable(string file)
		{
			var evt = DataAvailable;
			if(evt != null) evt(this, new FileSystemEventArgs(WatcherChangeTypes.Created,Path.GetDirectoryName(file),Path.GetFileName(file)));
		}
		protected override int ManagerCallbackInterval
		{
			get { return 60 * 1000; }
		}
	}
}
