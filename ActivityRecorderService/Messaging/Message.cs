using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tct.ActivityRecorderService.WebsiteServiceReference
{
	public partial class Message
	{
		[XmlIgnore]
		public bool ExpiryInHoursSpecified { get; set; }
	}
}
