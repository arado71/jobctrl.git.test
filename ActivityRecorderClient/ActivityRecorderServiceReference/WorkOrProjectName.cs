using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class WorkOrProjectName
	{
		public WorkOrProjectName()
		{
		}

		public WorkOrProjectName(WorkDataWithParentNames workData)
		{
			Id = workData.WorkData.Id;
			CategoryId = workData.WorkData.CategoryId;
			Name = workData.WorkData.Name;
			ParentId = workData.ParentId;
			ProjectId = workData.WorkData.ProjectId;
		}
	}
}
