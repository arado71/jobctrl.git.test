using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[Flags]
	public enum ClientDataCollectionSettings
	{
		None = 0,
		Url = 1,
		WindowTitle = 2,
		Email = 4,
		DocumentNameAndPath = 8,
		MobileLocation = 16,
		PhoneNumber = 32,
		Other = 64,
	}
}
