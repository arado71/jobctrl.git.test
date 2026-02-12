using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public class HourMinutePicker : TextBox
	{
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if ((e.KeyChar < '0' || e.KeyChar > '9') && (e.KeyChar != ':' || Text.Contains(':')) && e.KeyChar != 8)
			{
				e.Handled = true;
			}

			base.OnKeyPress(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			if (Value != null) Value = Value;

			base.OnLeave(e);
		}

		public bool ValidInput { get; private set; }

		public TimeSpan? Value
		{
			get
			{
				if (string.IsNullOrEmpty(Text))
				{
					ValidInput = true;
					return null;
				}

				var m = Regex.Match(Text, "^([0-9]*):([0-5]?[0-9]?)$");
				if (m.Success)
				{
					ValidInput = true;
					return new TimeSpan(int.Parse("0" + m.Groups[1].Value), int.Parse("0" + m.Groups[2].Value), 0);
				}

				m = Regex.Match(Text, "^[0-9]+$");
				if (m.Success)
				{
					ValidInput = true;
					return new TimeSpan(int.Parse(Text), 0, 0);
				}

				ValidInput = false;
				return null;
			}

			set
			{
				Text = value.HasValue ? string.Format("{0}:{1}", (int)value.Value.TotalHours, value.Value.Minutes.ToString("00")) : string.Empty;
			}
		}

		public HourMinutePicker()
		{
			ValidInput = true;
		}
	}
}
