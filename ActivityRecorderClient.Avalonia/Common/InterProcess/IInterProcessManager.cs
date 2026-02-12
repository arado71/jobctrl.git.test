using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.InterProcess
{
	public interface IInterProcessManager
	{
		void Start();
		void Stop();
		void UpdateMenu(ActivityRecorderServiceReference.ClientMenu clientMenu);
	}
}
