using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	public interface IMenuPublisher
	{
		void PublishMenu(ClientMenu clientMenu, Action<Exception> onErrorCallback);
	}
}
