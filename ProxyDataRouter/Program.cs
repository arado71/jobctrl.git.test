using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ProxyDataRouter
{
	class Program
	{

		static void Main(string[] args)
		{
			DataRouterRunner.Start(true);
		}
	}
}
