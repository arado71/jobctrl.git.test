using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MetroFramework;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Properties;
using Message = System.Windows.Forms.Message;
using Timer = System.Windows.Forms.Timer;

namespace Tct.ActivityRecorderClient.View
{
	public partial class FirstTimeWelcomeForm : FixedMetroForm, IMessageFilter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private int page = 0;
		private int lastSelected = -1;
		private readonly int pageCnt;
		private readonly int pageMax;
		private readonly PictureBox[] dotButtons;
		readonly Timer delayTimerForAltText1 = new Timer { Enabled = false };
		readonly Timer delayTimerForAltText2 = new Timer { Enabled = false };
		private Action recordAgree;
		private int instructionViewHeight;

		private class Instruction
		{
			public Image Image { get; set; }
			public string Text { get; set; }
			public string AltText1 { get; set; }
			public int AltText1Pos { get; set; }
			public int AltText1Delay { get; set; }
			public string AltText2 { get; set; }
			public int AltText2Delay { get; set; }
			public int AltText2Pos { get; set; }
			public bool HasAltText { get { return !string.IsNullOrEmpty(AltText1); } }
			public bool IsLargeText { get; set; }
			public bool ShowWorkModes { get; set; }
		};

		private readonly Instruction[]  baseInstrs = {
			new Instruction { Image = Resources.demo_1, Text = Labels.WelcomeInstructionText1, ShowWorkModes = true },
			new Instruction { Image = Resources.demo_2, Text = Labels.WelcomeInstructionText2 },
			new Instruction { Image = Resources.demo_3, Text = Labels.WelcomeInstructionText3 },
			new Instruction { Image = Resources.demo_4, Text = Labels.WelcomeInstructionText4, IsLargeText = true },
			new Instruction { Image = Resources.demo_5, AltText1 = Labels.WelcomeInstructionText5a, AltText1Pos = 135, AltText1Delay = 900,
														AltText2 = Labels.WelcomeInstructionText5b, AltText2Pos = 219, AltText2Delay = 2200 },
			new Instruction { Image = Resources.demo_6, AltText1 = Labels.WelcomeInstructionText6a, AltText1Pos = 107, AltText1Delay = 600,
														AltText2 = Labels.WelcomeInstructionText6b, AltText2Pos = 169, AltText2Delay = 2800 },
		};

		private readonly List<Instruction> instructions;

		public event EventHandler<SingleValueEventArgs<bool>> AfterClose;
		public FirstTimeWelcomeForm()
		{
			InitializeComponent();
		}

		public FirstTimeWelcomeForm(bool dontShow, AcceptanceData dppData, Action recordAgree)
		{
			instructions = new List<Instruction>(baseInstrs);
			if (dppData != null)
			{
				instructions.Insert(0, new Instruction() { Image = Resources.demo_0, Text = dppData.Message, IsLargeText = true });
				this.recordAgree = recordAgree;
			}
			pageCnt = instructions.Count;
			pageMax = pageCnt - 1;
			dotButtons = new PictureBox[pageCnt];
			InitializeComponent();
			LoadLocalization();
			Icon = Resources.JobCtrl;
			pbRed.Image = new Icon(Resources.NotWorking, 16, 16).ToBitmap();
			pbGreen.Image = new Icon(Resources.WorkingOnline, 16, 16).ToBitmap();
			pbYellow.Image = new Icon(Resources.WorkingOffline, 16, 16).ToBitmap();
			if (Labels.Culture.Name == "pt-BR" || Labels.Culture.Name == "es-MX")
			{
				lblRedDesc.FontSize = MetroLabelSize.Small;
				lblGreenDesc.FontSize = MetroLabelSize.Small;
				lblYellowDesc.FontSize = MetroLabelSize.Small;
				if (Labels.Culture.Name == "pt-BR")
				{
					lblRedDesc.Top -= 10;
					lblRedDesc.Height += 10;
				}
			}
			instructionViewHeight = pnlInstructionView.Height;
			delayTimerForAltText1.Tick += (sender, args) => { delayTimerForAltText1.Stop(); lblInstructionTextAlt1.Visible = instructions[page].HasAltText; };
			// Refresh needed in some cases when gif's last frame can't displayed
			delayTimerForAltText2.Tick += (sender, args) => { delayTimerForAltText2.Stop(); lblInstructionTextAlt2.Visible = instructions[page].HasAltText; pbInstruction.Refresh(); };
			CreateDotButtons();
			chbDontShow.Checked = dontShow;
			pnlWorkModes.BringToFront();
			RefreshPage();
			pbInstruction.Select();
		}

		private void CreateDotButtons()
		{
			flpDotButtons.Width = Resources.dot.Width*pageCnt;
			flpDotButtons.Left = Width/2 - flpDotButtons.Width/2;
			for (int i = 0; i < pageCnt; i++)
			{
				dotButtons[i] = new PictureBox() { Image = i > 0 ? Resources.dot : Resources.dot_selected, Width = Resources.dot.Width, Height = Resources.dot.Height, Margin = new Padding(0), Tag = i };
				flpDotButtons.Controls.Add(dotButtons[i]);
				dotButtons[i].Click += DotButtonClick;
			}
		}

		void DotButtonClick(object sender, EventArgs e)
		{
			var dotButton = sender as PictureBox;
			if (dotButton == null) return;
			if (recordAgree != null && page == 0) return;
			page = (int)dotButton.Tag;
			RefreshPage();
		}

		private void LoadLocalization()
		{
			Text = string.Format(Labels.WelcomeTitle, ConfigManager.AppNameOverride ?? ConfigManager.ApplicationName);
			lblWelcomeText.Text = string.Format(Labels.WelcomeCommonText, pageCnt);
			chbDontShow.Text = Labels.WelcomeDontShow;
			btnBack.Text = Labels.WelcomeBack.ToUpper();
			lblRedDesc.Text = Labels.WelcomeInstructionText1a;
			lblGreenDesc.Text = Labels.WelcomeInstructionText1b;
			lblYellowDesc.Text = Labels.WelcomeInstructionText1c;
		}

		protected virtual void OnAfterClose(bool flag)
		{
			var handler = AfterClose;
			if (handler != null) handler(this, new SingleValueEventArgs<bool>(flag));
		}

		private void FirstTimeWelcomeFormFormClosed(object sender, FormClosedEventArgs e)
		{
			Application.RemoveMessageFilter(this);
			OnAfterClose(chbDontShow.Checked);
		}

		private static readonly Regex anchorRegex = new Regex("<a href=\"(?<link>[^\"]*)\">(?<title>[^<]*)</a>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		private void SetText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				lblInstructionText.Text = string.Empty;
				lblInstructionText.Links.Clear();
				return;
			}
			var sb = new StringBuilder();
			var matches = anchorRegex.Matches(text);
			var pos = 0;
			var links = matches.Cast<Match>().Select(m =>
			{
				sb.Append(text.Substring(pos, m.Index - pos));
				var start = sb.Length;
				sb.Append(m.Groups["title"]);
				pos = m.Index + m.Length;
				return new { Url = m.Groups["link"].Value, Title = m.Groups["title"].Value, StartPosition = start, m.Groups["title"].Length };
			}).ToList();
			sb.Append(text.Substring(pos));
			lblInstructionText.Text = sb.ToString();
			lblInstructionText.Links.Clear();
			foreach (var link in links)
			{
				lblInstructionText.Links.Add(link.StartPosition, link.Length, link.Url);
			}
		}

		private void RefreshPage()
		{
			if (lastSelected == page) return;
			var instruction = instructions[page];
			pbInstruction.Image = instruction.Image;
			SetText(instruction.Text);
			btnBack.Visible = page > 0;
			btnOk.Text = page == 0 && recordAgree != null ? Labels.WelcomeAgree.ToUpper() : page < pageMax ? Labels.WelcomeNext.ToUpper() : Labels.WelcomeDone.ToUpper();
			chbDontShow.Visible = page > 0 || recordAgree == null;
			btnOk.Width = btnOk.Text.Length < 8 ? 75 : 150;
			if (instruction.HasAltText)
			{
				pnlInstructionView.Visible = false;
				lblInstructionTextAlt1.Visible = false;
				lblInstructionTextAlt2.Visible = false;
				lblInstructionTextAlt1.Text = instruction.AltText1;
				lblInstructionTextAlt1.Top = instruction.AltText1Pos;
				delayTimerForAltText1.Interval = instruction.AltText1Delay;
				delayTimerForAltText1.Start();
				lblInstructionTextAlt2.Text = instruction.AltText2;
				lblInstructionTextAlt2.Top = instruction.AltText2Pos;
				delayTimerForAltText2.Interval = instruction.AltText2Delay;
				delayTimerForAltText2.Start();
			}
			else
			{
				pnlInstructionView.Visible = true;
				lblInstructionTextAlt1.Visible = false;
				lblInstructionTextAlt2.Visible = false;
				delayTimerForAltText1.Stop();
				delayTimerForAltText2.Stop();
			}
			pnlWorkModes.Visible = instruction.ShowWorkModes;
			if (instruction.IsLargeText)
			{
				pnlInstructionView.Height = instructionViewHeight;
				var size = TextRenderer.MeasureText(lblInstructionText.Text, lblInstructionText.Font, new Size(lblInstructionText.Width, 0), TextFormatFlags.WordBreak);
				if (size.Height > 176)
				{
					scrInstructionView.SetScrollSize(pnlInstructionView.Height, size.Height);
					scrInstructionView.Visible = true;
				}
				lblInstructionText.Height = size.Height;
			}
			else
			{
				pnlInstructionView.Height = 
				lblInstructionText.Height = 76;
				scrInstructionView.Visible = false;
			}
			lblInstructionText.Top = 0;
			if (lastSelected >= 0) dotButtons[lastSelected].Image = Resources.dot;
			dotButtons[page].Image = Resources.dot_selected;
			lastSelected = page;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (page == 0 && recordAgree != null)
			{
				recordAgree();
				// only one times
				recordAgree = null;
			}
			if (page < pageMax)
			{
				page++;
				RefreshPage();
				pbInstruction.Select();
			}
			else
				Close();
		}

		private void chbImmStartWork_CheckedChanged(object sender, EventArgs e)
		{
			pbInstruction.Select();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyValue)
		{
			switch (keyValue)
			{
				case Keys.Escape:
					Close();
					return true;
				case Keys.Left:
				case Keys.Back:
				case Keys.Delete:
					if (page > 0)
					{
						page--;
						RefreshPage();
					}
					return true;
				case Keys.Right:
				case Keys.Space:
					if (page < pageMax && !(recordAgree != null && page == 0))
					{
						page++;
						RefreshPage();
					}
					return true;
			}
			return base.ProcessCmdKey(ref msg, keyValue);
		}

		private void btnBack_Click(object sender, EventArgs e)
		{
			if (page <= 0) return;
			page--;
			RefreshPage();
			pbInstruction.Select();
		}

		private void lblInstructionText_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!e.Button.HasFlag(MouseButtons.Left) || !(e.Link.LinkData is string)) return;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var url = "";
				try
				{
					url = e.Link.LinkData as string;
					if (!url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
					{
						var ticket = AuthenticationHelper.GetAuthTicket();
						url = string.Format(ConfigManager.WebsiteUrl + "Account/Login.aspx?ticket={0}&url=", ticket) + url;
					}
					var sInfo = new ProcessStartInfo(url);
					Process.Start(sInfo);
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (!pnlInstructionView.CheckCursorIsInsideControl()) return false;

			if (m.Msg == (int)WinApi.Messages.WM_MOUSEWHEEL)
			{
				var scrollDelta = (short)(((long)m.WParam >> 16) & 0xffff);
				scrollDelta = (short)((scrollDelta < 0 ? scrollDelta - 2 : scrollDelta + 2) / 3);
				//take one third of the original value
				scrInstructionView.ScrollDelta(scrollDelta);
				return true;
			}
			return false;
		}

		private void scrInstructionView_ScrollChanged(object sender, EventArgs e)
		{
			lblInstructionText.Location = new Point(0, -scrInstructionView.Value);
		}

		private void FirstTimeWelcomeForm_Load(object sender, EventArgs e)
		{
			Application.AddMessageFilter(this);
		}
	}
}
