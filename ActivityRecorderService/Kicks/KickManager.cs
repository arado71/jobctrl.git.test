using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.Kicks
{
	/// <summary>
	/// Thread-safe class for managing kicks.
	/// </summary>
	/// <remarks>
	/// Kicks are only read from the DB in the ctor. There are no periodic refreshes from it.
	/// </remarks>
	public class KickManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<ClientComputerKick> activeKicks = new List<ClientComputerKick>();
		private readonly KickCoordinator kickCoordinator = new KickCoordinator();
		private readonly Dictionary<int, Dictionary<long, ActiveDevice>> activeDevicesDict = new Dictionary<int, Dictionary<long, ActiveDevice>>();
		private readonly object thisLock = new object();

		public KickManager()
		{
			ManagerCallbackInterval = 1000; //check for expiration in every second
			lock (thisLock)
			{
				foreach (var activeKick in KickDbHelper.GetActiveKicks())
				{
					//activeKicks.Add(activeKick);
					kickCoordinator.AddKick(activeKick.Id);
				}
				var devs = KickDbHelper.GetActiveDevices();
				devs.ForEach(d => d.LastSeen = DateTime.UtcNow); // we think all devices are up to date because lastseen is not updated in db while pending
				foreach (var activeDevice in devs)
				{
					Dictionary<long, ActiveDevice> devices;
					if (!activeDevicesDict.TryGetValue(activeDevice.UserId, out devices))
					{
						devices = new Dictionary<long, ActiveDevice>();
						activeDevicesDict.Add(activeDevice.UserId, devices);
					}
					ActiveDevice dev;
					if (!devices.TryGetValue(activeDevice.DeviceId, out dev))
					{
						devices.Add(activeDevice.DeviceId, activeDevice);
					}
					else
					{
						log.Error("Removing duplicate ActiveDevice entry for user " + activeDevice.UserId + " and device " + activeDevice.DeviceId);
						KickDbHelper.RemoveDevice(activeDevice.UserId, activeDevice.DeviceId);
					}
				}
			}
		}

		public ClientComputerKick GetPendingKick(int userId, long deviceId)
		{
			ClientComputerKick pendingKick;
			lock (thisLock)
			{
				RemoveExpired(); //remove old entries
				pendingKick = activeKicks
					.Where(n => n.UserId == userId && n.ComputerId == deviceId)
					.OrderBy(n => n.Id)
					.FirstOrDefault();
				Dictionary<long, ActiveDevice> devices;
				ActiveDevice current;
				if (activeDevicesDict.TryGetValue(userId, out devices) && devices.TryGetValue(deviceId, out current))
				{
					current.LastSeen = DateTime.UtcNow;
					// KickDbHelper.UpdateDevice(userId, deviceId); //maybe skip this update if performance issues
				}
			}
			if (pendingKick != null)
			{
				KickDbHelper.SendKick(pendingKick.Id, userId, deviceId, DateTime.UtcNow); //we can try to send several times only the first will update the db
			}
			return pendingKick;
		}

		public void ConfirmKick(int userId, long deviceId, int kickId, KickResult result)
		{
			//we can confirm expired kicks as well
			if (KickDbHelper.ConfirmKick(kickId, userId, deviceId, DateTime.UtcNow, result) != 1) return; //return if already confirmed
			lock (thisLock)
			{
				kickCoordinator.TrySetKickResult(kickId, result);
				for (int i = 0; i < activeKicks.Count; i++)
				{
					if (activeKicks[i].Id == kickId)
					{
						activeKicks.RemoveAt(i);
						break;
					}
				}
				RemoveUserDevice(userId, deviceId);
			}
		}

		private void RemoveUserDevice(int userId, long deviceId)
		{
			lock (thisLock)
			{
				Dictionary<long, ActiveDevice> devices;
				if (!activeDevicesDict.TryGetValue(userId, out devices)) return;
				ActiveDevice current;
				if (!devices.TryGetValue(deviceId, out current)) return;
				devices.Remove(deviceId);
				KickDbHelper.RemoveDevice(userId, deviceId);
				if (devices.Count == 0) activeDevicesDict.Remove(userId);
			}
		}

		public KickResult? KickUserComputer(int userId, long deviceId, string reason, TimeSpan expiration, int createdBy, bool waitForResult = true)
		{
			var newKick = KickDbHelper.CreateKick(userId, deviceId, reason, expiration, createdBy);
			lock (thisLock)
			{
				kickCoordinator.AddKick(newKick.Id);
				activeKicks.Add(newKick);
			}
			KickResult? result;
			return waitForResult && kickCoordinator.TryGetKickResult(newKick.Id, out result) ? result : null;
		}

		private void RemoveExpired()
		{
			lock (thisLock)
			{
				for (int i = 0; i < activeKicks.Count; i++) //remove old entries
				{
					if (activeKicks[i].ExpirationDate < DateTime.UtcNow)
					{
						kickCoordinator.TrySetKickResult(activeKicks[i].Id, null); //timed out
						activeKicks.RemoveAt(i);
						i--;
					}
				}
				var thres = DateTime.UtcNow - TimeSpan.FromSeconds(ConfigManager.ClientKickTimeoutInSec);
				var devs2Remove = activeDevicesDict.Values.SelectMany(r => r.Values.Where(i => i.LastSeen < thres)).ToList();
				devs2Remove.ForEach(d =>
					{
						var devs = activeDevicesDict[d.UserId];
						devs.Remove(d.DeviceId);
						if (devs.Count == 0) activeDevicesDict.Remove(d.UserId);
						KickDbHelper.RemoveDevice(d.UserId, d.DeviceId);
					});
			}
		}

		protected override void ManagerCallbackImpl()
		{
			RemoveExpired();
		}

		public void MakeClientActive(int userId, long deviceId, bool isActive, TimeSpan expiration)
		{
			if (isActive)
			{
				lock (thisLock)
				{
					Dictionary<long, ActiveDevice> devices;
					if (!activeDevicesDict.TryGetValue(userId, out devices))
					{
						devices = new Dictionary<long, ActiveDevice>();
						activeDevicesDict[userId] = devices;
					}
					var clients = devices.Values.Where(n => n.DeviceId != deviceId).ToList();
					ActiveDevice current;
					if (devices.TryGetValue(deviceId, out current))
					{
						KickDbHelper.UpdateDevice(userId, deviceId);
					}
					else
					{
						var device = new ActiveDevice
						{
							DeviceId = deviceId,
							UserId = userId,
							FirstSeen = DateTime.UtcNow,
							LastSeen = DateTime.UtcNow
						};
						devices.Add(deviceId, device);
						KickDbHelper.AddDevice(device);
					}
					if (clients.Count == 0) return;
					ThreadPool.QueueUserWorkItem(_ =>
					{
						foreach (var client in clients)
						{
							try
							{
								KickUserComputer(userId, client.DeviceId, "", expiration, -1, false);
							}
							catch (Exception ex)
							{
								log.Error($"KickUserComputer failed (userId: {userId} device: {client.DeviceId}", ex);
							}
						}
					});
				}
			}
			else
			{
				RemoveUserDevice(userId, deviceId);
			}
		}
	}
}
