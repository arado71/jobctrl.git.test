using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public class TextBoxWithTagSuggestion : TextBox
	{
		private static readonly char[] listSeparators = new char[] { ' ', ',', ';', '|', '\n' };
		private ListBox cmsParticiantSuggestion;
		private bool listBoxAdded;
		private Point parentOffset; 

		public List<string> RecentTags { get; set; }

		public TextBoxWithTagSuggestion()
		{
			RecentTags = new List<string>();
			cmsParticiantSuggestion = new ListBox();
			cmsParticiantSuggestion.KeyDown += CmsParticiantSuggestionOnKeyDown;
			cmsParticiantSuggestion.Visible = false;
			cmsParticiantSuggestion.MouseClick += cmsParticiantSuggestion_MouseClick;
			cmsParticiantSuggestion.LostFocus += CmsParticiantSuggestionOnLostFocus;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					HideSuggestions();
					return;
				case Keys.Enter:
				case Keys.Up:
				case Keys.Down:
					if (cmsParticiantSuggestion.Items.Count > 0)
					{
						cmsParticiantSuggestion.SelectedIndex = 0;
						if (cmsParticiantSuggestion.Items.Count > 1)
						{
							cmsParticiantSuggestion.Focus();
						}
						else
							SuggestionSelected();
						e.Handled = true;
					}
					return;
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (e.KeyChar == '\n'
			    && cmsParticiantSuggestion.Focused)
				e.Handled = true; // to eliminate extra line break after entering suggestion list
			base.OnKeyPress(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down) return;
			var firstPos = Text.Substring(0, SelectionStart).LastIndexOfAny(listSeparators);
			var lastPos = Text.Substring(SelectionStart).IndexOfAny(listSeparators);
			if (firstPos < 0)
				firstPos = 0;
			else
				firstPos++;
			if (lastPos < 0)
				lastPos = Text.Length;
			else
				lastPos += SelectionStart;
			if (lastPos <= firstPos)
			{
				HideSuggestions();
				return;
			}
			var part = Text.Substring(firstPos, lastPos - firstPos);
			cmsParticiantSuggestion.Items.Clear();
			var lst = RecentTags.Where(i => i.StartsWith(part)).OrderBy(i => i).ToList();
			foreach (var tag in lst)
				cmsParticiantSuggestion.Items.Add(tag);
			if (cmsParticiantSuggestion.Items.Count == 0 || cmsParticiantSuggestion.Items.Count == 1 && cmsParticiantSuggestion.Items[0].Equals(part))
			{
				HideSuggestions();
				return;
			}
			Point cursorPos = Point.Empty;
			WinApi.GetCaretPos(ref cursorPos);
			var sugLoc = cursorPos;
			sugLoc.Offset(0, cmsParticiantSuggestion.ItemHeight);
			cmsParticiantSuggestion.Height = cmsParticiantSuggestion.ItemHeight * cmsParticiantSuggestion.Items.Count + 7;
			if (!listBoxAdded)
			{
				parentOffset = Location;
				var parent = Parent;
				while (parent is TableLayoutPanel || parent.Height < cmsParticiantSuggestion.Height + Height)
				{
					parentOffset.Offset(parent.Location);
					parent = parent.Parent;
				}
				if (parent.Height < parentOffset.Y + sugLoc.Y + cmsParticiantSuggestion.Height) parentOffset.Y += cursorPos.Y - cmsParticiantSuggestion.Height - sugLoc.Y;
				parent.Controls.Add(cmsParticiantSuggestion);
				cmsParticiantSuggestion.BringToFront();
				listBoxAdded = true;
			}
			sugLoc.Offset(parentOffset);
			cmsParticiantSuggestion.Location = sugLoc;
			if (cmsParticiantSuggestion.Left + cmsParticiantSuggestion.Width > cmsParticiantSuggestion.Parent.Width) cmsParticiantSuggestion.Left = cmsParticiantSuggestion.Parent.Width - cmsParticiantSuggestion.Width;
			cmsParticiantSuggestion.Show();
			Focus();
			base.OnKeyUp(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			if (!cmsParticiantSuggestion.Focused)
				HideSuggestions();
			base.OnLostFocus(e);
		}

		protected override void OnTabIndexChanged(EventArgs e)
		{
			cmsParticiantSuggestion.TabIndex = TabIndex + 1;
			base.OnTabIndexChanged(e);
		}

		void cmsParticiantSuggestion_MouseClick(object sender, MouseEventArgs e)
		{
			SuggestionSelected();
		}

		private void SuggestionSelected()
		{
			if (cmsParticiantSuggestion.SelectedIndex < 0) return;
			var firstPos = Text.Substring(0, SelectionStart).LastIndexOfAny(listSeparators);
			var lastPos = Text.Substring(SelectionStart).IndexOfAny(listSeparators);
			var partsBldr = new StringBuilder();
			if (firstPos >= 0)
				partsBldr.Append(Text.Substring(0, firstPos + 1));
			partsBldr.Append(cmsParticiantSuggestion.SelectedItem);
			var selStrt = partsBldr.Length + 1;
			if (lastPos < 0)
				partsBldr.Append(listSeparators[0]);
			else
			{
				lastPos += SelectionStart;
				partsBldr.Append(Text.Substring(lastPos));
			}
			Text = partsBldr.ToString();
			SelectionStart = selStrt;
			SelectionLength = 0;
			Focus();
			HideSuggestions();
		}

		private void CmsParticiantSuggestionOnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Focus();
				HideSuggestions();
				return;
			}
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return || e.KeyCode == Keys.Space)
			{
				SuggestionSelected();
				return;
			}
			if (char.IsLetterOrDigit((char)e.KeyCode))
			{
				var key = ((char) e.KeyCode).ToString();
				if (!e.Shift)
					key = key.ToLower();
				Focus();
				HideSuggestions();
				e.Handled = true;
				SendKeys.Send(key);
			}
		}

		private void CmsParticiantSuggestionOnLostFocus(object sender, EventArgs eventArgs)
		{
			if (!Focused)
				HideSuggestions();
		}

		private void HideSuggestions()
		{
			cmsParticiantSuggestion.Hide();
			cmsParticiantSuggestion.Items.Clear();
		}

	}
}
