using System;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public interface IDropdown
	{
		event EventHandler Hidden;
		bool IsShown { get; }
	}
}