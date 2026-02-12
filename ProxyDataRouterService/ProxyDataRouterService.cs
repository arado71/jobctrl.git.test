using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ProxyDataRouterService
{
	public partial class ProxyDataRouterService : ServiceBase
	{
		public ProxyDataRouterService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			ProxyDataRouter.DataRouterRunner.Start();
		}

		protected override void OnStop()
		{
			ProxyDataRouter.DataRouterRunner.Stop();
		}
	}
}
