using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Menu.Selector
{
	public interface IWorkSelectorService
	{
		event EventHandler<SingleValueEventArgs<WorkDataWithParentNames>> WorkSelected;
		void ShowSelectWorkGui(ClientMenuLookup menuLookup, string title, string description);
		void UpdateMenu(ClientMenuLookup menuLookup);
	}
}
