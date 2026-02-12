using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.Tests.ActivityRecorderClient
{
	public abstract class DisabledThreadAsserts : IDisposable
	{
		private readonly IDisposable disableThreadAsserts;

		protected DisabledThreadAsserts()
		{
			disableThreadAsserts = DebugEx.DisableThreadAsserts();
		}

		public void Dispose()
		{
			disableThreadAsserts.Dispose();
		}
	}
}
