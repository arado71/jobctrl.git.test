using System;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public interface ISelectable<T>
	{
		event EventHandler SelectionChanged;
		bool Selected { get; set; }
		T Value { get; set; }
	}
}