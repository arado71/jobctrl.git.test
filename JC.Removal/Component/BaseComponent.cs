using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JC.Removal.Component
{
	abstract class BaseComponent
	{
		public abstract string GetUserFriendlyName();
		public abstract bool Remove(out string error);
		public abstract bool IsInstalled();
		public abstract string[] GetProcessesNames();
	}
}
