using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderClient.Menu.Selector
{
	public class DefaultWorkIdSelector
	{
		private readonly SynchronizationContext context;
		private readonly IWorkSelectorService workSelectorService;
		private WorkDataWithParentNames selectedWork;
		private ClientMenuLookup menuLookup = new ClientMenuLookup();

		public DefaultWorkIdSelector(SynchronizationContext guiSyncContext)
		{
			context = guiSyncContext;
			workSelectorService = Platform.Factory.GetWorkSelectorService();
			workSelectorService.WorkSelected += WorkSelectorServiceWorkSelected;
		}

		//called from the GUI thread
		private void WorkSelectorServiceWorkSelected(object sender, SingleValueEventArgs<WorkDataWithParentNames> e)
		{
			Interlocked.Exchange(ref selectedWork, e.Value);
		}

		//called from BG or GUI thread
		public int? GetDefaultWorkId()
		{
			var curr = Interlocked.CompareExchange(ref selectedWork, null, null);
			if (curr != null) return curr.WorkData.Id.Value;
			ShowSelectWorkGui();
			return null;
		}

		private void ShowSelectWorkGui()
		{
			context.Post(_ =>
							{
								if (Interlocked.CompareExchange(ref selectedWork, null, null) != null) return;
								workSelectorService.ShowSelectWorkGui(menuLookup, Labels.WorkSelector_DefaultWorkTitle, Labels.WorkSelector_DefaultWorkBody);
							}, null);
		}

		//called from the GUI thread
		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			this.menuLookup = menuLookup;
			if (Interlocked.CompareExchange(ref selectedWork, null, null) != null)
			{
				Interlocked.Exchange(ref selectedWork, menuLookup.GetWorkDataWithParentNames(selectedWork.WorkData.Id.Value));
			}
			workSelectorService.UpdateMenu(menuLookup);
		}
	}
}
