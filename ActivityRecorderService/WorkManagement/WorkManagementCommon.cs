using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.WorkManagement
{
	[Flags]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum ProjectManagementPermissions
	{
		None = 0,
		CreateWork = 1 << 0,
		ModifyWork = 1 << 1,
		CloseWork = 1 << 2,
	}

	[Flags]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum ManagementFields
	{
		None = 0,
		Priority = 1 << 0,       //P,W,A (Projects,Works,Assignments)
		StartEndDate = 1 << 1,   //P,W,A
		TargetWorkTime = 1 << 2, //P,W,A
		TargetCost = 1 << 3,     //P,W
		Description = 1 << 4,    //P,W
		Category = 1 << 5,       //P,W
	}
}
