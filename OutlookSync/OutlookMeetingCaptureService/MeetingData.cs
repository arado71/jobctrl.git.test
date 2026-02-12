using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookMeetingCaptureService
{
	[Serializable]
	public class MeetingData: IEquatable<MeetingData>
	{
		public DateTime CreationTime { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Location { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public HashSet<string> Attendees { get; set; }

		public bool Equals(MeetingData other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return CreationTime.Equals(other.CreationTime) && string.Equals(Title, other.Title) && string.Equals(Description, other.Description) && string.Equals(Location, other.Location) && StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime) 
				&& (Attendees == null && other.Attendees == null
					|| Attendees?.Count == other.Attendees.Count && Attendees.SetEquals(other.Attendees));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MeetingData) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = CreationTime.GetHashCode();
				hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
				hashCode = (hashCode * 397) ^ EndTime.GetHashCode();
				hashCode = (hashCode * 397) ^ (Attendees != null ? Attendees.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}
