using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public interface INavigationWithWork
	{
		WorkDataWithParentNames Work { get; }
	}
}
