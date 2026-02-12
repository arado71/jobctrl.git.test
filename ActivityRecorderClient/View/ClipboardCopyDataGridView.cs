using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public class ClipboardCopyDataGridView : DataGridView
	{
		public event EventHandler<ClipboardCopyEventArgs> ClipboardCopy;

		public override DataObject GetClipboardContent()
		{
			EventHandler<ClipboardCopyEventArgs> handler = ClipboardCopy;
			var orig = base.GetClipboardContent();
			if (orig == null)
			{
				//we don't want to override this (CurrentCellIsEditedAndOnlySelectedCell)
				return null;
			}
			if (handler == null)
			{
				return orig;
			}
			else
			{
				var e = new ClipboardCopyEventArgs();
				handler(this, e);
				return e.ClipboardData;
			}
		}
	}

	public class ClipboardCopyEventArgs : EventArgs
	{
		public DataObject ClipboardData { get; set; }
	}
}
