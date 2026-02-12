using System;
using System.Windows.Forms;
using MetroFramework;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public enum SmartTextBoxType
	{
		String,
		PositiveInteger,
		PositiveIntegerAndMinusOne,
		PositiveReal
	}

	public partial class SmartTextBox : UserControl
	{
		private bool hasFocus = false;
		private string invalidMessage;
		private bool isValid = true;
		private string lastAcceptedValue = string.Empty;
		private string originalValue;
		private Func<string, bool> textInvariant = x => true;
		private SmartTextBoxType type;
		private Func<string, bool> validator = x => true;

		public event EventHandler TextSaved;

		public override string Text
		{
			get { return txtInput.Text; }

			set
			{
				SetText(value);
				originalValue = value;
				lastAcceptedValue = value;
			}
		}

		public SmartTextBoxType InputType
		{
			get { return type; }

			set
			{
				type = value;
				switch (type)
				{
					case SmartTextBoxType.String:
						validator = _ => true;
						textInvariant = _ => true;
						invalidMessage = string.Empty;
						break;
					case SmartTextBoxType.PositiveInteger:
						validator = ValidatorPositiveInteger;
						textInvariant = x => ValidatorPositiveInteger(x) || string.IsNullOrEmpty(x);
						invalidMessage = Labels.Preference_NotPositiveInteger;
						break;
					case SmartTextBoxType.PositiveIntegerAndMinusOne:
						validator = ValidatorPositiveAndMinusOneInteger;
						textInvariant = x => ValidatorPositiveAndMinusOneInteger(x) || string.IsNullOrEmpty(x);
						invalidMessage = Labels.Preference_NotPositiveInteger;
						break;
					case SmartTextBoxType.PositiveReal:
						validator = ValidatorPositiveReal;
						textInvariant = x => ValidatorPositiveReal(x) || string.IsNullOrEmpty(x);
						invalidMessage = Labels.Preference_NotPositiveReal;
						break;
				}
			}
		}

		public SmartTextBox()
		{
			InitializeComponent();
			Localize();
			txtInput.Font = MetroFonts.Default(12);
		}

		private void HandleInputChanged(object sender, EventArgs e)
		{
			if (hasFocus)
			{
				if (textInvariant(txtInput.Text))
				{
					lastAcceptedValue = txtInput.Text;
				}
				else
				{
					SetText(lastAcceptedValue);
				}

				pbIcon.Image = Resources.pencil;
				if (txtInput.Text == originalValue)
				{
					pbIcon.Visible = false;
				}
				else
				{
					ttIcon.SetToolTip(pbIcon, Labels.Preference_EditText);
					pbIcon.Visible = true;
				}
			}
		}

		private void HandleInputFocus(object sender, EventArgs e)
		{
			hasFocus = true;
		}

		private void HandleInputFocusLost(object sender, EventArgs e)
		{
			hasFocus = false;
			SaveValue();
		}

		private void HandleKeyPressing(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
			{
				SaveValue();
			}

			if (e.KeyCode == Keys.Escape)
			{
				DiscardValue();
			}
		}

		private void DiscardValue()
		{
			SetText(originalValue);
		}

		private void Localize()
		{
			ttIcon.SetToolTip(pbIcon, Labels.Preference_EditText);
		}

		private void SaveValue()
		{
			isValid = validator(txtInput.Text);
			if (isValid)
			{
				pbIcon.Visible = false;
				if (originalValue != Text)
				{
					if (TextSaved != null) TextSaved(this, EventArgs.Empty);
				}

				originalValue = txtInput.Text;
			}
			else
			{
				pbIcon.Image = Resources.warning;
				ttIcon.SetToolTip(pbIcon, invalidMessage);
			}
		}

		private void SetText(string text)
		{
			int lastSelection = txtInput.Text.Length - txtInput.SelectionStart;
			txtInput.Text = text;
			txtInput.Select(text.Length - lastSelection, 0);
		}

		private bool ValidatorPositiveInteger(string input)
		{
			int num;
			if (!int.TryParse(input, out num)) return false;
			return num >= 0;
		}

		private bool ValidatorPositiveAndMinusOneInteger(string input)
		{
			int num;
			if (!int.TryParse(input, out num) && input != "-") return false;
			return num >= -1;
		}

		private bool ValidatorPositiveReal(string input)
		{
			double num;
			if (!double.TryParse(input, out num)) return false;
			return num >= 0;
		}
	}
}