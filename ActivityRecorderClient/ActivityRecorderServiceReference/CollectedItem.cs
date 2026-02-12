using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class CollectedItem : IUploadItem
	{
		public Guid Id { get; set; }

		public DateTime StartDate
		{
			get { return CreateDate; }
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("Cr: ").Append(CreateDate.ToString()).Append(" V: ");
			if (CapturedValues == null)
			{
				sb.Append("(null)");
			}
			else
			{
				var isFirst = true;
				foreach (var kvp in CapturedValues)
				{
					sb.Append(isFirst ? "" : ", ").Append(kvp.Key).Append(":").Append(kvp.Value);
					isFirst = false;
				}
			}
			return sb.ToString();
		}
	}
}
