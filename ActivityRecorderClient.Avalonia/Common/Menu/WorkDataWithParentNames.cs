using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	[DataContract]
	public class WorkDataWithParentNames
	{
		public static readonly IEqualityComparer<WorkDataWithParentNames> WorkDataIdComparer = new WorkDataWithParentNamesIdComparer();
		public static readonly IEqualityComparer<WorkDataWithParentNames> WorkDataProjectIdComparer = new WorkDataWithParentNamesProjectIdComparer();

		public static readonly string DefaultSeparator = " \u00BB ";
		public static readonly string NewLineSeparator = " \u00BB\n";

		[DataMember]
		public int? ParentId { get; set; }
		[DataMember]
		public WorkData WorkData { get; set; }
		[DataMember]
		public List<string> ParentNames { get; set; }
		[IgnoreDataMember]
		public string FullName
		{
			get
			{
				return GetFullName(DefaultSeparator, DefaultSeparator);
			}
		}

		public string GetFullName(string projSeparator, string workSeparator)
		{
			return
				((ParentNames == null || ParentNames.Count == 0)
					? ""
					: (string.Join(projSeparator, ParentNames.ToArray()) + workSeparator))
				+ (WorkData == null ? null : WorkData.Name);
		}

		private class WorkDataWithParentNamesIdComparer : IEqualityComparer<WorkDataWithParentNames>
		{
			#region IEqualityComparer<WorkDataWithParentNames> Members

			public bool Equals(WorkDataWithParentNames x, WorkDataWithParentNames y)
			{
				if (x == null) return y == null;
				if (y == null) return false;
				if (x.WorkData == null) return y.WorkData == null;
				if (y.WorkData == null) return false;
				return x.WorkData.Id == y.WorkData.Id;
			}

			public int GetHashCode(WorkDataWithParentNames obj)
			{
				if (obj == null) return 0;
				if (obj.WorkData == null) return 1;
				return obj.WorkData.Id.GetHashCode();
			}

			#endregion
		}

		private class WorkDataWithParentNamesProjectIdComparer : IEqualityComparer<WorkDataWithParentNames>
		{
			#region IEqualityComparer<WorkDataWithParentNames> Members

			public bool Equals(WorkDataWithParentNames x, WorkDataWithParentNames y)
			{
				if (x == null) return y == null;
				if (y == null) return false;
				if (x.WorkData == null) return y.WorkData == null;
				if (y.WorkData == null) return false;
				return x.WorkData.ProjectId == y.WorkData.ProjectId;
			}

			public int GetHashCode(WorkDataWithParentNames obj)
			{
				if (obj == null) return 0;
				if (obj.WorkData == null) return 1;
				return obj.WorkData.ProjectId.GetHashCode();
			}

			#endregion
		}

		public override string ToString()
		{
			return (WorkData == null
				? ""
				: (FullName
					+ (WorkData.Id.HasValue
						? " (" + WorkData.Id + ")"
						: "")));
		}
	}
}
