using System;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public interface IDropdownContainer
	{
		event EventHandler DropdownClosed;
		bool DropdownShown { get; }
		void RegisterDropdown(IDropdown dropdown);
	}
}