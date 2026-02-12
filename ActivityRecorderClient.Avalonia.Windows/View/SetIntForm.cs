using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class SetIntForm : Form
	{
		public string Title
		{
			get { return this.Text; }
			set
			{
				this.Text = value;
				this.lblValueTitle.Text = value;
			}
		}

		private int value;
		public int Value
		{
			get { return value; }
			set
			{
				this.value = value;
				txtValue.Text = "" + value;
			}
		}

		public Func<int, bool> Validation { get; set; }
		public string ValidationErrorText { get; set; }

		public SetIntForm()
		{
			InitializeComponent();
			this.SetFormStartPositionCenterScreen();

			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			ActiveControl = txtValue;

			btnCancel.Text = Labels.Cancel;
			lblValueText.Text = Labels.Value + ":";
			//this.ControlBox = false; //hax to avoid lag from UserActivityHook
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			int parsed;
			if (string.IsNullOrEmpty(txtValue.Text) || !int.TryParse(txtValue.Text, out parsed))
			{
				MessageBox.Show(Labels.IntForm_PleaseEnterANumber);
				return;
			}
			if (Validation != null && !Validation(parsed))
			{
				MessageBox.Show(Labels.IntForm_InvalidNumber + ": " + ValidationErrorText);
				return;
			}
			Value = parsed;
			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void txtValue_TextChanged(object sender, EventArgs e)
		{
			txtValue.Text = Regex.Replace(txtValue.Text, "[^0-9]", "");
		}

		private void txtValue_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
			if (e.KeyChar == (char)Keys.Return)
			{
				btnOk_Click(sender, e);
			}
		}
	}
}
