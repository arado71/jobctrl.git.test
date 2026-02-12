using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Update
{
	public interface IAutoStartHelper
	{
		void Register(IUpdateService updateService);
	}
}
