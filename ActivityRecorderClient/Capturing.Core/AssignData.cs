using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Class uniquely identifying a dynamic task locally.
	/// </summary>
	/// <remarks>Holds either an AssignWorkData or an AssignProjectData or an AssignCompositeData (but only one)</remarks>
	[Serializable]
	public class AssignData : IEquatable<AssignData>
	{
		// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
		public AssignWorkData Work { get; private set; }
		public AssignProjectData Project { get; private set; }
		public AssignCompositeData Composite { get; private set; }
		public AssignCommonData Common { get; private set; }
		// ReSharper restore AutoPropertyCanBeMadeGetOnly.Local

		public AssignData(AssignWorkData work)
		{
			Work = work ?? throw new ArgumentNullException(nameof(work));
		}

		public AssignData(AssignProjectData project)
		{
			Project = project ?? throw new ArgumentNullException(nameof(project));
		}

		public AssignData(AssignCompositeData composite)
		{
			Composite = composite ?? throw new ArgumentNullException(nameof(composite));
		}

		public AssignData(AssignCommonData common)
		{
			SetAssignData(common);
		}

		public void SetAssignData(AssignCommonData common)
		{
			Common = common ?? throw new ArgumentNullException(nameof(common));
		}

		public bool Equals(AssignData other)
		{
			if (other == null) return false;
			return (Work != null && Work.Equals(other.Work))
				|| (Project != null && Project.Equals(other.Project))
				|| (Composite != null && Composite.Equals(other.Composite))
				|| (Common != null && Common.Equals(other.Common))
				;
		}

		public override bool Equals(object obj)
		{
			var other = obj as AssignData;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			var result = 17;
			// ReSharper disable NonReadonlyMemberInGetHashCode
			result = 31 * result + (Work == null ? 0 : Work.GetHashCode());
			result = 31 * result + (Project == null ? 0 : Project.GetHashCode());
			result = 31 * result + (Composite == null ? 0 : Composite.GetHashCode());
			result = 31 * result + (Common == null ? 0 : Common.GetHashCode());
			// ReSharper restore NonReadonlyMemberInGetHashCode
			return result;
		}

		public override string ToString()
		{
			return (Work?.ToString() ?? "")
				+ (Project?.ToString() ?? "")
				+ (Composite?.ToString() ?? "")
				+ (Common?.ToString() ?? "")
				;
		}
	}
}
