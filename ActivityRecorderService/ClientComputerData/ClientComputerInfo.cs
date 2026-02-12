using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.ClientComputerData
{
	public partial class ClientComputerInfo
	{
		public override string ToString()
		{
			return String.Format("User: {0} Comp: {1} OS: v{2}.{3}.{4}.{5} Net4: {6} Net45: {7} Computername: {8} Username: {9}", UserId, ComputerId, OSMajor, OSMinor, OSBuild, OSRevision, IsNet4Available, IsNet45Available, MachineName, LocalUserName);
		}
	}
}
