using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	[Serializable]
	public class LocationKey : IEquatable<LocationKey>
	{
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public enum LocationKeyType
		{
			Work = 0,
			Project,
			MenuItem,
			ClosedWork,
		}

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public enum MenuItems
		{
			Root = -1,
			Priority = -2,
			Deadline = -3,
			Progress = -4,
			Favorite = -5,
			Suggestion = -6,
			Recent = -7,
			All = -8,
			RecentProject = -9,
			RecentClosed = -10,
		}

		internal int Id { get; private set; }
		internal LocationKeyType Type { get; private set; }

		public static readonly LocationKey Root = new LocationKey((int)MenuItems.Root, LocationKeyType.MenuItem);
		public static readonly LocationKey Priority = new LocationKey((int)MenuItems.Priority, LocationKeyType.MenuItem);
		public static readonly LocationKey Deadline = new LocationKey((int)MenuItems.Deadline, LocationKeyType.MenuItem);
		public static readonly LocationKey Progress = new LocationKey((int)MenuItems.Progress, LocationKeyType.MenuItem);
		public static readonly LocationKey Favorite = new LocationKey((int)MenuItems.Favorite, LocationKeyType.MenuItem);
		public static readonly LocationKey Suggestion = new LocationKey((int)MenuItems.Suggestion, LocationKeyType.MenuItem);
		public static readonly LocationKey Recent = new LocationKey((int)MenuItems.Recent, LocationKeyType.MenuItem);
		public static readonly LocationKey All = new LocationKey((int)MenuItems.All, LocationKeyType.MenuItem);
		public static readonly LocationKey RecentProject = new LocationKey((int)MenuItems.RecentProject, LocationKeyType.MenuItem);
		public static readonly LocationKey RecentClosed = new LocationKey((int)MenuItems.RecentClosed, LocationKeyType.MenuItem);

		public static LocationKey CreateFrom(WorkData data)
		{
			Debug.Assert(data.Id != null || data.ProjectId != null);
			return data.Id.HasValue
				? new LocationKey(data.Id.Value, LocationKeyType.Work)
				: new LocationKey(data.ProjectId.Value, LocationKeyType.Project);
		}

		public static LocationKey CreateClosed(WorkDataWithParentNames data)
		{
			Debug.Assert(data != null && data.WorkData != null && data.WorkData.Id != null);
			return new LocationKey(data.WorkData.Id.Value, LocationKeyType.ClosedWork);
		}

		private LocationKey(int id, LocationKeyType type)
		{
			Id = id;
			Type = type;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LocationKey);
		}

		public bool Equals(LocationKey other)
		{
			if (ReferenceEquals(other, null)) return false;
			return Id == other.Id && Type == other.Type;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode(); //id is almost unique
		}

		public override string ToString()
		{
			switch (Type)
			{
				case LocationKeyType.MenuItem:
					return string.Format("MenuItem - {0}", (MenuItems)Id);
				case LocationKeyType.Project:
					return string.Format("Project - {0}", Id);
				case LocationKeyType.Work:
					return string.Format("Work - {0}", Id);
			}

			Debug.Fail("Unknown LocationKey type");
			return string.Format("Unkown {1} - {0}", Id, Type);
		}
	}
}