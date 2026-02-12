using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class AssignWorkData : IEquatable<AssignWorkData>
	{
		[DataMember]
		public int? WorkId { get; set; }

		public bool Equals(AssignWorkData other)
		{
			if (other == null) return false;
			return string.Equals(WorkKey, other.WorkKey, StringComparison.OrdinalIgnoreCase)
				&& ProjectId == other.ProjectId
				&& ServerRuleId == other.ServerRuleId;
		}

		public override bool Equals(object obj)
		{
			var other = obj as AssignWorkData;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (WorkKey == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(WorkKey));
			result = 31 * result + (ProjectId == null ? 0 : ProjectId.GetHashCode());
			result = 31 * result + ServerRuleId.GetHashCode();
			return result;
		}

		public override string ToString()
		{
			return "WorkKey: " + WorkKey + " Name: " + WorkName + " Proj: " + ProjectId + " sid: " + ServerRuleId + " d: " + Description;
		}
	}
}
