using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class RootMenuHelper
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static List<LocationKey> menuItems = null;
		private static HashSet<LocationKey> menuItemHash = null;

		public static event EventHandler ListChanged;

		private static string MenuItemFile
		{
			get { return "RootMenu-" + ConfigManager.UserId; }
		}

		public static HashSet<LocationKey> GetMenuItemHashes()
		{
			LoadMenuItems();
			return menuItemHash;
		}

		public static IEnumerable<LocationKey> GetMenuItems()
		{
			LoadMenuItems();
			return menuItems;
		}

		public static bool IsMenuItem(LocationKey key)
		{
			LoadMenuItems();
			return menuItemHash.Contains(key);
		}

		public static void Move(LocationKey key, int position)
		{
			if (!menuItemHash.Contains(key)) return;
			menuItems.Remove(key);
			if (position >= menuItems.Count)
			{
				menuItems.Add(key);
			}
			else
			{
				menuItems.Insert(position, key);
			}

			SaveMenuItems();
			OnListChanged();
		}

		public static void SetMenuItem(LocationKey key, bool value)
		{
			LoadMenuItems();
			if (!value && menuItemHash.Contains(key))
			{
				menuItems.Remove(key);
				menuItemHash.Remove(key);
				SaveMenuItems();
				OnListChanged();
			}

			if (value && !menuItemHash.Contains(key))
			{
				menuItems.Insert(0, key);
				menuItemHash.Add(key);
				SaveMenuItems();
				OnListChanged();	
			}
		}

		private static void OnListChanged()
		{
			EventHandler evt2 = ListChanged;
			if (evt2 != null) evt2(null, EventArgs.Empty);
		}

		private static void LoadMenuItems()
		{
			if (menuItems == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(MenuItemFile))
				{
					IsolatedStorageSerializationHelper.Load(MenuItemFile, out menuItems);
				}
				if (menuItems == null) menuItems = new List<LocationKey>
				{
					LocationKey.Favorite, 
					LocationKey.Recent, 
					LocationKey.RecentProject,
					//LocationKey.RecentClosed,
					LocationKey.Deadline, 
					LocationKey.Priority, 
					LocationKey.Progress, 
					LocationKey.All, 
				};
				menuItemHash = new HashSet<LocationKey>(menuItems);
			}
		}

		private static void SaveMenuItems()
		{
			if (menuItems != null)
			{
				IsolatedStorageSerializationHelper.Save(MenuItemFile, menuItems);
			}
		}
	}
}