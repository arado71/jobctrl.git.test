using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class NewWorkHelper
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private const int RecentLength = 3;
		private const int RepresentativeLength = 2;

		private static List<NewWorkStatistic> recents = null;

		private static string NewWorkStatPath
		{
			get { return "NewWorkStats-" + ConfigManager.UserId; }
		}

		public static bool AddRecent(WorkData work, int projectId, bool startNew)
		{
			LoadRecents();
			recents.Insert(0, new NewWorkStatistic(work, projectId) { StartNew = startNew });
			if (recents.Count > RecentLength)
			{
				recents.RemoveAt(recents.Count - 1);
			}

			SaveRecents();
			return true;
		}

		public static void Clear()
		{
			recents = new List<NewWorkStatistic>();
			SaveRecents();
		}

		public static NewWorkStatistic GetRepresentative()
		{
			LoadRecents();
			var result = new NewWorkStatistic()
			{
				CategoryId = MostAtLeastElement(recents.Select(x => x.CategoryId), RepresentativeLength),
				Description = MostAtLeastElement(recents.Select(x => x.Description), RepresentativeLength),
				Length = MostAtLeastElement(recents.Select(x => x.Length), RepresentativeLength),
				Priority = MostAtLeastElement(recents.Select(x => x.Priority), RepresentativeLength),
				ProjectId = MostAtLeastElement(recents.Select(x => x.ProjectId), RepresentativeLength),
				StartOffset = MostAtLeastElement(recents.Select(x => x.StartOffset), RepresentativeLength),
				TargetWorkTime = MostAtLeastElement(recents.Select(x => x.TargetWorkTime), RepresentativeLength),
				StartNew = MostAtLeastElement(recents.Select(x => x.StartNew), RepresentativeLength)
			};
			return result;
		}

		private static T MostAtLeastElement<T>(IEnumerable<T> elements, int minimumCount)
		{
			var keys = new List<T>();
			var values = new List<int>();
			foreach (var element in elements)
			{
				if (!keys.Contains(element))
				{
					keys.Add(element);
					values.Add(0);
				}

				values[keys.IndexOf(element)]++;
			}

			var max = int.MinValue;
			var mostElement = default(T);
			var keyEnum = keys.GetEnumerator();
			var valEnum = values.GetEnumerator();
			while (keyEnum.MoveNext())
			{
				var valMoveResult = valEnum.MoveNext();
				Debug.Assert(valMoveResult);
				if (valEnum.Current < minimumCount || valEnum.Current <= max) continue;

				max = valEnum.Current;
				mostElement = keyEnum.Current;
			}

			return mostElement;
		}

		public static NewWorkStatistic[] GetRecents()
		{
			LoadRecents();
			return recents.ToArray();
		}

		private static void LoadRecents()
		{
			if (recents == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(NewWorkStatPath))
				{
					IsolatedStorageSerializationHelper.Load(NewWorkStatPath, out recents);
				}

				if (recents == null) recents = new List<NewWorkStatistic>();
			}
		}

		private static void SaveRecents()
		{
			if (recents != null)
			{
				IsolatedStorageSerializationHelper.Save(NewWorkStatPath, recents);
			}
		}
	}
}