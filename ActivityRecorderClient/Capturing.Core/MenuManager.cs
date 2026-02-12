using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Retrives ClientMenu from the service and persits it
	/// </summary>
	public class MenuManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public string MenuFile { get { return "ClientMenu-" + ConfigManager.UserId; } }
		public event EventHandler<MenuEventArgs> CurrentMenuChanged;

		private string currentMenuVersion;

		private ClientMenu currentMenu;
		public ClientMenu CurrentMenu
		{
			get { return currentMenu; }
			private set
			{
				if (value == null) //cannot save null value
				{
					CurrentMenu = new ClientMenu();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(currentMenu, value)) return;
				log.Info("Menu changed");
				var oldMenu = currentMenu;
				currentMenu = value;
				IsolatedStorageSerializationHelper.Save(MenuFile, currentMenu);
				RaiseCurrentMenuChanged(currentMenu, oldMenu);
			}

		}

		public MenuManager()
			: base(log)
		{
		}

		//todo save/load in xml format
		public void LoadMenu()
		{
			if (IsolatedStorageSerializationHelper.Exists(MenuFile))
			{
				ClientMenu menu;
				if (IsolatedStorageSerializationHelper.Load(MenuFile, out menu))
				{
					//CurrentMenu ??
					currentMenu = menu;
				}
			}
		}

		public void RefreshMenu()
		{
			RestartTimer();
		}

		protected override int ManagerCallbackInterval
		{
			get { return ConfigManager.MenuUpdateInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				string newVersion = null;
				var clientMenu = ActivityRecorderClientWrapper.Execute(n => n.GetClientMenu(out newVersion, userId, currentMenuVersion));
				if (newVersion != currentMenuVersion)
				{
					log.Debug("New version. (" + currentMenuVersion + " -> " + newVersion + ")");
					currentMenuVersion = newVersion;
					CurrentMenu = clientMenu;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get client menu", log, ex);
			}
		}

		private void RaiseCurrentMenuChanged(ClientMenu menu, ClientMenu oldMenu)
		{
			EventHandler<MenuEventArgs> updated = CurrentMenuChanged;
			if (updated == null) return;
			try
			{
				var e = new MenuEventArgs(menu, oldMenu);
				updated(this, e);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseCurrentMenuChanged", ex);
			}
		}

		//private void SaveCurrentMenu()
		//{
		//    string tmpFile = MenuFile + ".tmp";
		//    string bakFile = MenuFile + ".bak";
		//    try
		//    {
		//        XmlPersistenceManager.SaveToFile(tmpFile, currentMenu);
		//        if (File.Exists(MenuFile))
		//        {
		//            File.Replace(tmpFile, MenuFile, bakFile);
		//        }
		//        else
		//        {
		//            File.Move(tmpFile, MenuFile);
		//        }
		//        log.Info("Successfully saved menu file " + MenuFile);
		//    }
		//    catch (PlatformNotSupportedException)
		//    {
		//        if (File.Exists(bakFile))
		//        {
		//            File.Delete(bakFile);
		//        }
		//        File.Move(MenuFile, bakFile);
		//        File.Move(tmpFile, MenuFile);
		//    }
		//    catch (Exception ex)
		//    {
		//        log.Error("Unable to save menu file " + MenuFile, ex);
		//    }
		//}
	}
}
