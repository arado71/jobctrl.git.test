using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class TelemetryItem : IUploadItem
	{
		public Guid Id { get; set; }
	}
}
