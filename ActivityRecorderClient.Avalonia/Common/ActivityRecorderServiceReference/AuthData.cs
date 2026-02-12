using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class AuthData
	{
		public string FullName => string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) || Labels.Culture == null ? Name : Labels.Culture.GetCultureSpecificName(FirstName, LastName);
	}
}
