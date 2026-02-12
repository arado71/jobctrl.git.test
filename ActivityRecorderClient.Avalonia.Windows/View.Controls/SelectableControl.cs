using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class SelectableControl<T> : UserControl, ISelectable<T> where T : IEquatable<T>
	{
		private T currentValue;
		protected bool selected = false;

		public event EventHandler SelectionChanged;

		public bool Selected
		{
			get { return selected; }

			set
			{
				if (IsDisposed) return;
				if (selected == value) return;
				selected = value;
				RenderSelection();
				RaiseSelectionChanged();
			}
		}

		public T Value
		{
			get { return currentValue; }

			set
			{
				if (EqualityComparer<T>.Default.Equals(currentValue, value)) return;
				currentValue = value;
				RenderValue();
			}
		}

		protected virtual void RenderSelection()
		{
		}

		protected virtual void RenderValue()
		{
		}

		private void RaiseSelectionChanged()
		{
			EventHandler evt = SelectionChanged;
			if (evt != null) evt(this, EventArgs.Empty);
		}
	}
}