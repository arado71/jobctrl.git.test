using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.View
{
	public class ComboBoxElement
	{
		public string Text { get; set; }
		public object Value { get; set; }
		
		public ComboBoxElement(string display, object value)
		{
			this.Text = display;
			this.Value = value;
		}

		public override string ToString()
		{
			return this.Text;
		}
	}
}
