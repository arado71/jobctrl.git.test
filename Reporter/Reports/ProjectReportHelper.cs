using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Reports
{
	public class ProjectReportHelper
	{
		public static string Serialize(ProjectNode rootNode)
		{
			var ser = new DataContractJsonSerializer(typeof(ProjectNode), null, int.MaxValue, true, null, false); //extension data is lost, but that is ok atm.
			using (var stream = new MemoryStream())
			{
				ser.WriteObject(stream, rootNode);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
	}
}
