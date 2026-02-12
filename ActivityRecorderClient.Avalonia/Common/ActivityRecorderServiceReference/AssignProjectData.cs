using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class AssignProjectData : IEquatable<AssignProjectData>
	{
		[DataMember]
		public int? WorkId { get; set; }
		[DataMember]
		public int? ProjectId { get; set; }

		public bool Equals(AssignProjectData other)
		{
			if (other == null) return false;
			return string.Equals(ProjectKey, other.ProjectKey, StringComparison.OrdinalIgnoreCase)
				&& ServerRuleId == other.ServerRuleId;
		}

		public override bool Equals(object obj)
		{
			var other = obj as AssignProjectData;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (ProjectKey == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(ProjectKey));
			result = 31 * result + ServerRuleId.GetHashCode();
			return result;
		}

		public override string ToString()
		{
			return "ProjectKey: " + ProjectKey + " Name: " + ProjectName + " sid: " + ServerRuleId;
		}
	}
}
