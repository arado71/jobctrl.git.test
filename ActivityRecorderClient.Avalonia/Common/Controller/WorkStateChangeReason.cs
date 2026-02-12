using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Controller
{
	public enum WorkStateChangeReason
	{ 
		UserSelect,
		UserResume,
		AutodetectedTemp,
		AutodetectedEndTempEffect,
		AutodetectedPerm,
		AutoResume		// resume due to user activity
	}
}
