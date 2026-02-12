using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using log4net;
using Microsoft.Win32;

namespace AppControlService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public class AppControlServiceService : IAppControlServiceService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Dictionary<int, ProcessData> processMap = new Dictionary<int, ProcessData>();
		private object lockObj = new object();

		public void RegisterProcess(int pid)
		{
			try
			{
				log.DebugFormat("RegisterProcess ({0}) started.", pid);
				lock (lockObj)
				{
					var process = Process.GetProcessById(pid);
					if (processMap.ContainsKey(pid))
					{
						ApplicationLoader.CloseHandle(processMap[pid].Token);
						processMap.Remove(pid);
						process.Exited -= ProcessOnExited;
					}
					ApplicationLoader.SECURITY_ATTRIBUTES secAttrs;
					var hUserTokenDup = (IntPtr) 0;
					ApplicationLoader.DuplicateToken((uint) pid, out secAttrs, ref hUserTokenDup);
					var data = new ProcessData
					{
						Pid = pid,
						Path = process.MainModule.FileName,
						Token = hUserTokenDup,
						SecAttrs = secAttrs,
					};
					processMap.Add(pid, data);
					process.EnableRaisingEvents = true;
					process.Exited += ProcessOnExited;
					log.DebugFormat("RegisterProcess ({0}) finished successfuly.", pid);
				}
			}
			catch (Exception e)
			{
				log.Error("RegisterProcess failed", e);
			}
		}

		private void ProcessOnExited(object sender, EventArgs eventArgs)
		{
			var process = sender as Process;
			if (process == null) return;
			try
			{
				log.DebugFormat("ProcessOnExited ({0}) started.", process.Id);
				process.Exited -= ProcessOnExited;
				ProcessData data;
				lock (lockObj)
				{
					if (!processMap.TryGetValue(process.Id, out data))
					{
						log.WarnFormat("Process {0} not found in process map", process.Id);
						return;
					}
				}
				var procInfo = new ApplicationLoader.PROCESS_INFORMATION();
				ApplicationLoader.CreateProcessAsUser(data.Path, data.Token, data.SecAttrs, ref procInfo);
				UnregisterProcess(process.Id);
				log.DebugFormat("ProcessOnExited ({0}) finished.", process.Id);
			}
			catch (Exception e)
			{
				log.Error("ProcessOnExited failed", e);
			}
			finally
			{
				process.Dispose();
			}
		}

		public void UnregisterProcess(int pid)
		{
			try
			{
				log.DebugFormat("UnregisterProcess ({0}) started.", pid);
				lock (lockObj)
				{
					if (processMap.ContainsKey(pid))
					{
						ApplicationLoader.CloseHandle(processMap[pid].Token);
						processMap.Remove(pid);
					}
				}
				log.DebugFormat("UnregisterProcess ({0}) finished successfuly.", pid);
			}
			catch (Exception e)
			{
				log.Error("UnregisterProcess failed", e);
			}
		}

		internal void CheckAndHandleClosed()
		{
			var pids = Process.GetProcesses().Select(p => p.Id).ToList();
			lock (lockObj)
			{
				foreach (var data in processMap.Values.Where(d => !pids.Contains(d.Pid)).ToList())
				{
					var procInfo = new ApplicationLoader.PROCESS_INFORMATION();
					ApplicationLoader.CreateProcessAsUser(data.Path, data.Token, data.SecAttrs, ref procInfo);
					UnregisterProcess(data.Pid);
				}
			}
		}
	}

	public class ProcessData
	{
		public int Pid { get; set; }
		public IntPtr Token { get; set; }
		public string Path { get; set; }
		public ApplicationLoader.SECURITY_ATTRIBUTES SecAttrs { get; set; }
	}
}
