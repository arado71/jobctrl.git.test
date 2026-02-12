using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	[ToolboxBitmap(typeof(CheckBox))]
	public partial class ToggleGroup : UserControl
	{
		public ToggleGroup()
		{
			InitializeComponent();
			cbToggle.CheckedChanged += cbToggle_CheckedChanged;
		}

		void cbToggle_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateState();
			if (this.CheckedChanged != null)
			{
				this.CheckedChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler CheckedChanged;

		public string Title
		{
			get
			{
				return lblTitle.Text;
			}

			set
			{
				lblTitle.Text = value;
				toolTip1.SetToolTip(lblTitle, value);
			}
		}

		private string textOn, textOff;

		private void UpdateState()
		{
			this.lblState.Text = this.cbToggle.Checked ? this.textOn : this.textOff;
		}

		public string TextOn
		{
			get
			{
				return this.textOn;
			}
			set
			{
				this.textOn = value;
				this.UpdateState();
			}
		}

		public string TextOff
		{
			get
			{
				return this.textOff;
			}
			set
			{
				this.textOff = value;
				this.UpdateState();
			}
		}

		public bool Checked
		{
			get
			{
				return this.cbToggle.Checked;
			}

			set
			{
				this.cbToggle.Checked = value;
				this.UpdateState();
			}
		}
	}
}
