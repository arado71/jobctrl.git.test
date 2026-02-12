using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ProjectSync
{
	public interface IProjectSyncService
	{
		void ShowSync();
		void ShowInfo(string text);
	}
}
