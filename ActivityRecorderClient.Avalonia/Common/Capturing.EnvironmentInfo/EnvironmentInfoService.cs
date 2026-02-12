using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.EnvironmentInfo
{
	public abstract class EnvironmentInfoService : IEnvironmentInfoService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public abstract RemoteDesktop GetCurrentRemoteDesktop();
		protected abstract VirtualMachine GetCurrentVirtualMachine();
		protected abstract bool GetIsRemoteDesktop();
		protected abstract int GetComputerId();

		private readonly int computerId; //intiated once in ctor

		protected EnvironmentInfoService()
		{
			OSVersion = Environment.OSVersion.Version;
			OSFullName = null;
			computerId = GetComputerIdOrMinusOneNoThrow(false); //DANGER virtual call in ctor
			if (ComputerId != -1) return;
			int tries = 40;
			while (computerId == -1 && --tries > 0)
			{
				if (tries % 5 == 0) log.Info("Still calculating computer id");
				System.Threading.Thread.Sleep(2000);
				computerId = GetComputerIdOrMinusOneNoThrow(tries == 1); //log error on last
			}
		}

		public int ComputerId
		{
			get { return computerId; }
		}

		public bool IsRemoteDesktop
		{
			get { return GetIsRemoteDesktopOrFalseNoThrow(); }
		}

		private readonly object isVirtualMachineLock = new object();
		private volatile bool isVirtualMachineInited;
		private bool isVirtualMachineValue; //assume that IsVM can't change after we detected so cache it's value
		public bool IsVirtualMachine //Thread-safe lazy accessor (no .NET 4)
		{
			get
			{
				if (!isVirtualMachineInited)
				{
					lock (isVirtualMachineLock)
					{
						if (!isVirtualMachineInited)
						{
							VirtualMachine curretnVm;
							if (!TryGetCurrentVirtualMachine(out curretnVm))
								return false; //unable to detect so assume no vm, but retry on next query
							isVirtualMachineValue = curretnVm != VirtualMachine.None;
							isVirtualMachineInited = true;
						}
					}
				}
				return isVirtualMachineValue;
			}
		}

		private bool TryGetCurrentVirtualMachine(out VirtualMachine result)
		{
			try
			{
				result = GetCurrentVirtualMachine();
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Unable to get virtual machine info", ex);
				result = VirtualMachine.None;
				return false;
			}
		}

		private int GetComputerIdOrMinusOneNoThrow(bool logError)
		{
			try
			{
				return GetComputerId();
			}
			catch (Exception ex)
			{
				if (logError) log.Error("Unable to get computerId", ex);
				return -1;
			}
		}

		private bool GetIsRemoteDesktopOrFalseNoThrow()
		{
			try
			{
				return GetIsRemoteDesktop();
			}
			catch (Exception ex)
			{
				log.Error("Unable to get remote desktop info", ex);
				return false;
			}
		}

		public abstract bool IsNet4Available { get; }

		public abstract bool IsNet45Available { get; }
		public abstract int HighestNetVersionAvailable { get; }

		public Version OSVersion { get; protected set; }
		public string OSFullName { get; protected set; }
	}
}
