using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Communication;
using VoxCTRL.ActivityRecorderServiceReference;
using VoxCTRL.Serialization;

namespace VoxCTRL.Communication
{
	public class ClientSettingsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int settingsUpdateInterval = 60 * 1000;   //60 secs /**/7 GetClientSettings, 234 bytes/call inside, in variables; but 60 packets 23996 bytes/call outside, in Ethernet packets
		private static string settingsFile { get { return "ClientSettings-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<ClientSetting>> SettingsChanged;

		private volatile ClientSetting clientSettings;
		public ClientSetting ClientSettings
		{
			get { return clientSettings; }
			private set
			{
				if (value == null) //cannot save null value
				{
					ClientSettings = new ClientSetting();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(clientSettings, value)) return;
				log.Info("Client settings changed");
				clientSettings = value;
				IsolatedStorageSerializationHelper.Save(settingsFile, value);
			}
		}

		private string currentVersion;

		public ClientSettingsManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get
			{
				return settingsUpdateInterval;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				string newVersion = null;
				var settings = ActivityRecorderClientWrapper.Execute(n => n.GetClientSettings(out newVersion, userId, currentVersion));
				if (newVersion != currentVersion)
				{
					log.Debug("New version. (" + currentVersion + " -> " + newVersion + ")");
					currentVersion = newVersion;
					ClientSettings = settings;
					UpdateConfigManagerWithClientSettings(ClientSettings);
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get client settings", log, ex);
			}
		}

		public void LoadSettings()
		{
			if (IsolatedStorageSerializationHelper.Exists(settingsFile))
			{
				ClientSetting settings;
				if (IsolatedStorageSerializationHelper.Load(settingsFile, out settings))
				{
					log.Info("Loading client settings from disk");
					clientSettings = settings;
					UpdateConfigManagerWithClientSettings(settings);
				}
			}
		}

		private void UpdateConfigManagerWithClientSettings(ClientSetting settings)
		{
			if (settings == null)
			{
				UpdateConfigManagerWithClientSettings(new ClientSetting());
				return;
			}

			ConfigManager.IsNameMandatory = settings.VoxIsNameMandatory;
			ConfigManager.Quality = settings.VoxQuality;
			ConfigManager.IsManualStartStopEnabled = settings.VoxIsManualStartStopEnabled ?? true;

			OnSettingsChanged(settings);
		}

		private void OnSettingsChanged(ClientSetting settings)
		{
			var del = SettingsChanged;
			if (del != null) del(this, SingleValueEventArgs.Create(settings));
		}
	}
}
