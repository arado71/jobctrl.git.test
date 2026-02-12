using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class AssignCompositeData : IEquatable<AssignCompositeData>
	{
		public int? WorkId { get; set; }

		public bool Equals(AssignCompositeData other)
		{
			if (other == null) return false;
			return string.Equals(WorkKey, other.WorkKey, StringComparison.OrdinalIgnoreCase)
				//&& ProjectId == other.ProjectId
				&& ServerRuleId == other.ServerRuleId
				&& ProjectKeys.CollectionEqual(other.ProjectKeys, StringComparer.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			var other = obj as AssignCompositeData;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (WorkKey == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(WorkKey));
			//result = 31 * result + (ProjectId == null ? 0 : ProjectId.GetHashCode());
			result = 31 * result + ServerRuleId.GetHashCode();
			if (ProjectKeys != null && ProjectKeys.Count > 0)
			{
				foreach (var projectKey in ProjectKeys)
				{
					result = 31 * result + (projectKey == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(projectKey));
				}
			}
			return result;
		}

		public override string ToString()
		{
			return "Composite WorkKey: " + WorkKey + " Name: " + WorkName
				+ " P: " + (ProjectKeys == null ? "" : string.Join("\\", ProjectKeys.ToArray()))
				+ /*" Proj: " + ProjectId +*/ " sid: " + ServerRuleId;
		}
	}
}
