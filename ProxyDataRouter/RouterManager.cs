using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient;

namespace ProxyDataRouter
{
	class RouterManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public RouterManager() : base(log)
		{
			
		}

		protected override void ManagerCallbackImpl()
		{
			throw new NotImplementedException();
		}

		protected override int ManagerCallbackInterval { get; }
	}
}
