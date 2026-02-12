using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public interface IStartable
	{
		void Start();
		void Stop();
	}
}
