using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class FavoritesHelper
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static List<int> favorites = null;
		private static HashSet<int> favoriteHash = null;
		private static WorkDataWithParentNames[] favoriteCache = null;
		private static long? favoriteVersion = null;

		public static event EventHandler ListChanged;

		private static string FavoriteFile
		{
			get { return "Favorites-" + ConfigManager.UserId; }
		}

		public static HashSet<int> GetFavoriteHashes()
		{
			LoadFavorites();
			return favoriteHash;
		}

		public static IEnumerable<int> GetFavoriteIds()
		{
			LoadFavorites();
			return favorites;
		}

		public static WorkDataWithParentNames[] GetFavorites()
		{
			if (IsCacheValid())
			{
				return favoriteCache;
			}

			Versionable<ClientMenuLookup> lookup = MenuQuery.Instance.ClientMenuLookup;
			favoriteVersion = lookup.Version;
			return favoriteCache = FetchFavorites(lookup.Value).ToArray();
		}

		public static bool IsFavorite(int id)
		{
			LoadFavorites();
			return favoriteHash.Contains(id);
		}

		public static void Move(int id, int position)
		{
			if (!favoriteHash.Contains(id)) return;
			favorites.Remove(id);
			if (position >= favorites.Count)
			{
				favorites.Add(id);
			}
			else
			{
				favorites.Insert(position, id);
			}

			favoriteVersion = null;
			SaveFavorites();
			OnListChanged();
		}

		public static bool ToggleFavorite(int workId)
		{
			LoadFavorites();
			favoriteVersion = null;
			if (favoriteHash.Contains(workId))
			{
				favorites.Remove(workId);
				favoriteHash.Remove(workId);
				SaveFavorites();
				OnListChanged();
				return false;
			}
			favorites.Insert(0, workId);
			favoriteHash.Add(workId);
			SaveFavorites();
			OnListChanged();
			return true;
		}

		private static void OnListChanged()
		{
			EventHandler evt2 = ListChanged;
			if (evt2 != null) evt2(null, EventArgs.Empty);
		}

		private static void Cleanup()
		{
			ClientMenuLookup lookup = MenuQuery.Instance.ClientMenuLookup.Value;
			favorites =
				new List<int>(favorites.Where(x => lookup.ProjectDataById.ContainsKey(x) || lookup.WorkDataById.ContainsKey(x)));
			favoriteHash = new HashSet<int>(favorites);
			OnListChanged();
		}

		private static IEnumerable<WorkDataWithParentNames> FetchFavorites(ClientMenuLookup lookup)
		{
			LoadFavorites();
			if (lookup == null)
			{
				yield break;
			}

			bool needCleanup = false;
			foreach (var favorite in favorites)
			{
				if (lookup.ProjectDataById.ContainsKey(favorite))
				{
					yield return lookup.ProjectDataById[favorite];
					continue;
				}

				if (lookup.WorkDataById.ContainsKey(favorite))
				{
					yield return lookup.WorkDataById[favorite];
					continue;
				}

				needCleanup = true;
				log.InfoFormat("Favorite {0} not found", favorite);
			}

			if (needCleanup) Cleanup();
		}

		private static bool IsCacheValid()
		{
			return favoriteCache != null && favoriteVersion != null && favoriteVersion.Value == MenuQuery.Instance.ClientMenuLookup.Version;
		}

		private static void LoadFavorites()
		{
			if (favorites == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(FavoriteFile))
				{
					IsolatedStorageSerializationHelper.Load(FavoriteFile, out favorites);
				}
				if (favorites == null) favorites = new List<int>();
				favoriteHash = new HashSet<int>(favorites);
			}
		}

		private static void SaveFavorites()
		{
			if (favorites != null)
			{
				IsolatedStorageSerializationHelper.Save(FavoriteFile, favorites);
			}
		}
	}
}