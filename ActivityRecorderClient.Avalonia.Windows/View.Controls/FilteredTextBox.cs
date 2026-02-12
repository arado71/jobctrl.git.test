using System;
using System.ComponentModel;
using System.Windows.Forms;
using MetroFramework;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed class FilteredTextBox : TextBox
	{
		private string lastAcceptedValue = string.Empty;

		public FilteredTextBox()
		{
			Font = MetroFonts.Default(12);
			Invariant = x => true;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<string, bool> Invariant { get; set; } 


		protected override void OnTextChanged(EventArgs e)
		{
			int lastSelection = Text.Length - SelectionStart;
			if (Invariant(Text))
			{
				lastAcceptedValue = Text;
			}
			else
			{
				Text = lastAcceptedValue;
			}
			
			Select(Text.Length - lastSelection, 0);
			base.OnTextChanged(e);
		}
	}
}