using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.View.Controls;
using Tct.ActivityRecorderClient.View.Presenters;

namespace Tct.ActivityRecorderClient.View
{
	public partial class OfflineWorkForm : FixedMetroForm, IOfflineWorkView, IMessageFilter, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int maxMeetingsViewHeight = 370;
		private static readonly Dictionary<CardStyle, Color> intervalColorMap = new Dictionary<CardStyle, Color>() { { CardStyle.Normal, Color.FromArgb(208, 236, 249) }, { CardStyle.Selected, Color.FromArgb(18, 156, 221) }, { CardStyle.Deleted, Color.FromArgb(240, 240, 240) }, { CardStyle.Incomplete, Color.FromArgb(255, 200, 200) }, { CardStyle.None, Color.White } };

		private readonly ActivityRecorderForm owner;
		private readonly OfflineWorkPresenter presenter;

		private Size meetingsTlpSize;
		private bool formIsActive;
		private int mWidth;

		public OfflineWorkForm()
		{
			InitializeComponent();
			tlpMeetings.RowStyles.RemoveAt(0);
			var card1 = new OfflineWorkCard() { BackColor = SystemColors.Control };
			tlpMeetings.Controls.Add(card1, 0, 0);
		}

		public OfflineWorkForm(IAdhocMeetingService service, Form owner)
		{
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			InitializeComponent();
			tlpMeetings.RowStyles.RemoveAt(0);
			tlpMeetings.RowCount = 0;
			tlpMeetings.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			Debug.Assert(owner is ActivityRecorderForm);
			this.owner = owner as ActivityRecorderForm;
			presenter = new OfflineWorkPresenter(this, service, Platform.Factory.GetAddressBookService());
			meetingsTlpSize = pnlMeetingView.Size;
			tlpMeetings.Height = 0;
			timeSplitter.IntervalSelected += TimeSplitterIntervalSelected;
			timeSplitter.SplitterTimeChanged += TimeSplitterSplitterTimeChanged;
			pnlMeetingView.VerticalScroll.Enabled = true;
			pnlMeetingView.VerticalScroll.Visible = false;
			Localize();
		}

		public void Localize()
		{
			Text = Labels.AddMeeting_Title;
			btnOk.Text = Labels.AddMeeting_ConfirmButton.ToUpper();
			btnCancel.Text = Labels.AddMeeting_AbortButton.ToUpper();
			toolTip.SetToolTip(btnOk, Labels.AddMeeting_ConfirmButtonTooltip);
			toolTip.SetToolTip(pbSplit, Labels.AddMeeting_Split);
			toolTip.SetToolTip(pbMerge, Labels.AddMeeting_Merge);
		}

		public OfflineWorkPresenter Presenter { get { return presenter; } }

		public void RunOnGui(Action action)
		{
			context.Post((_) => action(), null);
		}

		public void SetAlternativeMenu(Action<WorkDataEventArgs> click, string caption)
		{
			owner.SetAlternativeMenu(click, click, caption);
		}

		public void ShowView()
		{
			Show(owner);
		}

		public void PopupView()
		{
			WindowState = FormWindowState.Normal;
			if (!Visible)
				Show();
		}

		public void AbortAndClose(bool force)
		{
			log.Info("Called AbortAndClose" + (force ? " Forced" : ""));
			if (!force)
			{
				if (WindowState == FormWindowState.Minimized)
					WindowState = FormWindowState.Normal;
				DialogResult = DialogResult.Cancel;
			}
			else
				DialogResult = DialogResult.Abort;
			Close();
		}

		public void ShowMessageBox(string body, string title)
		{
			MessageBox.Show(this, body, title);
		}

		public void AddMeetingCard(MeetingInfo info, Guid after)
		{
			btnCancel.Select();
			var card = new OfflineWorkCard(context, info, presenter.CanSelectWork, presenter.RecentWorkIdsSelector) { BackColor = SystemColors.Control, Dock = DockStyle.Fill };
			if (after == Guid.Empty)
			{
				tlpMeetings.Controls.Add(card, 0, tlpMeetings.RowCount);
				if (tlpMeetings.RowCount == 0 && btnOk.Width + btnCancel.Width > 380) // only first time
				{
					var diff = btnOk.Width + btnCancel.Width - 300;
					meetingsTlpSize.Width += diff;
					Width += diff;
					card.Width += diff;
					mWidth = card.Width;
				}
				else if (mWidth > 0) card.Width = mWidth;
			}
			else
			{
				var pos = tlpMeetings.Controls.OfType<OfflineWorkCard>().Where(c => c.MeetingInfo.Id == after).Select(c => tlpMeetings.GetRow(c)).FirstOrDefault();
				foreach (var item in tlpMeetings.Controls.OfType<OfflineWorkCard>().Where(c => tlpMeetings.GetRow(c) > pos).OrderByDescending(c => tlpMeetings.GetRow(c)))
				{
					tlpMeetings.SetRow(item, tlpMeetings.GetRow(item) + 1);
				}
				tlpMeetings.Controls.Add(card, 0, pos + 1);
				if (mWidth > 0) card.Width = mWidth;
			}
			tlpMeetings.Height += card.Height + card.Margin.Top + card.Margin.Bottom;
			if (MinimumSize == Size.Empty)
			{
				Height -= pnlMeetingBlock.Height - tlpMeetings.Height;
				MinimumSize = Size;
			}
			UpdateSize();
			tlpMeetings.RowCount++;
			card.CardClicked += CardClicked;
			card.Validation += card_Validation;
			card.WorkSelectionChanged += card_WorkSelectionChanged;
			card.CardExpandStateChanged += card_CardExpandStateChanged;
			card.CardDeletedStateChanged += card_CardDeletedStateChanged;
			card.DataChanged += card_DataChanged;
			card.AddressbookSelected += card_AddressbookSelected;
			card.SizeChanged += card_SizeChanged;
			card.InitFieldValues();
			if (after == Guid.Empty)
			{
				scrMeetings.Value = tlpMeetings.Height - pnlMeetingBlock.Height; // go to end
				ScrollTo(scrMeetings.Value);
			}
		}

		public void DeleteMeetingCard(Guid id)
		{
			var found = tlpMeetings.Controls.OfType<OfflineWorkCard>().FirstOrDefault(ctrl => ctrl.Tag is Guid && (Guid) ctrl.Tag == id);
			if (found != null)
			{
				tlpMeetings.Controls.Remove(found);
				tlpMeetings.Height -= found.Height + found.Margin.Top + found.Margin.Bottom;
				UpdateSize();
			}
		}

		//protected override CreateParams CreateParams
		//{
		//	get
		//	{
		//		CreateParams cp = base.CreateParams;
		//		cp.ExStyle = cp.ExStyle | 0x2000000;
		//		return cp;
		//	}
		//}

		void card_CardExpandStateChanged(object sender, SingleValueEventArgs<bool> e)
		{
			if (!(sender is OfflineWorkCard card)) return;
			if (card.Top < -tlpMeetings.Top)
			{
				scrMeetings.Value = card.Top;
				ScrollTo(card.Top);
			}
			if (e.Value) // card just expanded
			{
				if (card.Bottom + tlpMeetings.Top > pnlMeetingBlock.Height)
				{
					scrMeetings.Value = card.Top - pnlMeetingBlock.Height + card.Height;
					ScrollTo(scrMeetings.Value);
				}
			}
		}

		void card_WorkSelectionChanged(object sender, SingleValueEventArgs<object> e)
		{
			if (!(sender is OfflineWorkCard card)) return;
			Presenter.WorkSelectionChanged(card.MeetingInfo, e.Value);
		}

		void card_Validation(object sender, EventArgs e)
		{
			if (!(sender is OfflineWorkCard card)) return;
			Presenter.ValidateInput(card.MeetingInfo);
		}

		private void card_CardDeletedStateChanged(object sender, SingleValueEventArgs<bool> e)
		{
			if (!(sender is OfflineWorkCard card)) return;
			Presenter.DeleteCard(card.MeetingInfo, e.Value);
		}

		private void card_DataChanged(object sender, EventArgs e)
		{
			if (!(sender is OfflineWorkCard card)) return;
			Presenter.MeetingInfoChanged(card.MeetingInfo);
		}

		private void card_AddressbookSelected(object sender, EventArgs e)
		{
			if (!(sender is OfflineWorkCard card)) return;
			Presenter.AddressbookSelected(card.MeetingInfo, Handle);
		}

		private void card_SizeChanged(object sender, EventArgs e)
		{
			var nHeight = tlpMeetings.Controls.OfType<OfflineWorkCard>().Sum(c => c.Height + c.Margin.Top + c.Margin.Bottom);
			if (tlpMeetings.Height == nHeight) return;
			tlpMeetings.Height = nHeight;
			UpdateSize();
		}

		private void TimeSplitterIntervalSelected(object sender, SingleValueEventArgs<int> e)
		{
			Presenter.IntervalSelected(e.Value);
		}

		private void TimeSplitterSplitterTimeChanged(object sender, SingleValueEventArgs<Tuple<int, DateTime>> e)
		{
			Presenter.IntervalTimeChanged(e.Value.Item1, e.Value.Item2);
		}

		public void UpdateTotal(string totalSumText)
		{
			lblSum.Text = totalSumText;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (!this.CheckCursorIsInsideControl()) return false;
			
			if (m.Msg == (int) WinApi.Messages.WM_MOUSEWHEEL)
			{
				var scrollDelta = (short) (((long) m.WParam >> 16) & 0xffff);
				scrollDelta = (short) ((scrollDelta < 0 ? scrollDelta - 2 : scrollDelta + 2)/3);
					//take one third of the original value
				scrMeetings.ScrollDelta(scrollDelta);
				return true;
			}
			return false;
		}


		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == (int)WinApi.Messages.WM_NCACTIVATE)
			{
				// more accurate retrieving of window active state
				// https://msdn.microsoft.com/en-us/library/windows/desktop/ms632633(v=vs.85).aspx
				var state = m.WParam.ToInt32();
				formIsActive = state > 0;
			}
		}

		public void HandleUserActivity(bool isMouseActivity)
		{
			log.DebugFormat("OnAutoReturnFromMeeting (mouseact: {0})", isMouseActivity);
			if (Bounds.Contains(Cursor.Position)) return;
			if (!isMouseActivity && formIsActive) return;
			if (owner.IsExcludedFromObserving(Cursor.Position)) return;
			Presenter.AutoReturnFromMeeting();
			TopMost = false;
		}

		public bool ShowCloseConfirmationDialog()
		{
			var res = MessageBox.Show(this,
				/* IsPoppedUpAfterInactivity ? Labels.AddMeeting_AbortConfirmationInactivity :*/ Labels.AddMeeting_AbortConfirmation,
				Labels.AddMeeting_AbortConfirmationTitle, MessageBoxButtons.OKCancel);
			log.Info("Closing form confirmation " + res);
			return res == DialogResult.OK;
		}

		public bool IsRecordActionAvailable
		{
			get => btnOk.Enabled;
			set => btnOk.Enabled = value;
		}

		public void UpdateStopWatch(bool visible)
		{
			if (pbCounter.Visible == visible) return;
			pbCounter.Visible = visible;
		}

		public int AddInterval(DateTime start, CardStyle type, bool isDraggable = false)
		{
			return timeSplitter.AddSplitter(start, intervalColorMap[type], isDraggable, type == CardStyle.None);
		}

		public void ModifyIntervalColor(int index, CardStyle type)
		{
			timeSplitter.SetSplitterBarColor(index, intervalColorMap[type]);
		}

		public void ModifyIntervalEnd(int index, bool isDraggable)
		{
			timeSplitter.SetSplitterEnd(index, isDraggable, !isDraggable);
		}

		public void ModifyIntervalTime(int index, DateTime time)
		{
			timeSplitter.SetSplitterStartTime(index, time);
		}

		public int InsertInterval(int index, DateTime start, CardStyle type, bool isDraggable)
		{
			return timeSplitter.InsertSplitter(index, start, intervalColorMap[type], isDraggable, type == CardStyle.None);
		}

		public void RemoveInterval(int index)
		{
			timeSplitter.RemoveSplitter(index);
		}

		public bool IsSplitButtonEnabled
		{
			get => pbSplit.Enabled;
			set
			{
				pbSplit.Enabled = value;
				pbSplit.Image = value ? Resources.split: Resources.split_disabled;
			}
		}

		public bool? IsMergeButtonEnabled
		{
			get => pbMerge.Visible ? (bool?) pbMerge.Enabled : null;
			set
			{
				pbMerge.Visible = value.HasValue;
				pbMerge.Enabled = value ?? false;
				pbMerge.Image = pbMerge.Enabled ? Resources.merge : Resources.merge_disabled;
			}
		}

		public bool IsIntervalSplitterEnabled
		{
			get => timeSplitter.Visible;
			set
			{
				if (timeSplitter.Visible == value) return;
				timeSplitter.Visible = value;
				var isMinHeight = Height == MinimumSize.Height;
				int nHeight;
				if (value)
					nHeight = Height + timeSplitter.Height + timeSplitter.Margin.Bottom + timeSplitter.Margin.Top;
				else
					nHeight = Height - timeSplitter.Height - timeSplitter.Margin.Bottom - timeSplitter.Margin.Top;
				if (isMinHeight) MinimumSize = new Size(MinimumSize.Width, nHeight);
				Height = nHeight;
			}
		}

		public void DropdownTaskList(Guid id)
		{
			var found = tlpMeetings.Controls.OfType<OfflineWorkCard>().FirstOrDefault(ctrl => ctrl.Tag is Guid && (Guid)ctrl.Tag == id);
			if (found == null) return;
			found.DropdownTaskList();
		}


		void CardClicked(object sender, EventArgs e)
		{
			var card = sender as OfflineWorkCard;
			Debug.Assert(card != null);
			Presenter.CardClicked(card.MeetingInfo);
		}

		public void ActivateView()
		{
			if (WindowState == FormWindowState.Minimized)
				WindowState = FormWindowState.Normal;
			Activate();
			TopMost = true;
		}

		private void OfflineWorkForm_Load(object sender, EventArgs e)
		{
			Application.AddMessageFilter(this);
		}

		private void OfflineWorkForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Debug.Assert(Presenter != null);
			if (Presenter != null)
				Presenter.ViewClosed(!owner.IsDuringExit && DialogResult == DialogResult.None ? OfflineWindowCloseReason.CancelWorks : Map(DialogResult));
			Application.RemoveMessageFilter(this);
		}

		private void UpdateSize()
		{
			if (MinimumSize.Height == Height)
			{
				var nHeight = Height + tlpMeetings.Height - pnlMeetingBlock.Height;
				if (nHeight < Screen.FromControl(this).Bounds.Height * 0.7)
				{
					MinimumSize = new Size(MinimumSize.Width, nHeight);
					Height = nHeight;
					return;
				}
			}
			UpdateScrollbar();
		}

		private void UpdateScrollbar()
		{
			if (!Visible) return;
			scrMeetings.SetScrollSize(pnlMeetingBlock.Height, tlpMeetings.Height);
			if (scrMeetings.Value > scrMeetings.ScrollTotalSize - scrMeetings.ScrollVisibleSize) scrMeetings.Value = scrMeetings.ScrollTotalSize - scrMeetings.ScrollVisibleSize;
			ScrollTo(scrMeetings.Value);
			meetingsTlpSize = new Size(tlpMeetings.Width, pnlMeetingBlock.Height);
		}

		private void HandleScrolled(object sender, EventArgs e)
		{
			ScrollTo(scrMeetings.Value);
		}

		private void ScrollTo(int verticalPosition)
		{
			foreach (var card in tlpMeetings.Controls.OfType<OfflineWorkCard>().Where(c => c.MeetingInfo.CardStyle == CardStyle.Selected))
			{
				card.DropdownTaskList(false);
			}
			tlpMeetings.Location = new Point(0, -verticalPosition);
		}

		private void OfflineWorkForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = !Presenter.ConfirmClosing(Map(DialogResult), owner.IsDuringExit);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private OfflineWindowCloseReason Map(DialogResult result)
		{
			switch (result)
			{
				case DialogResult.None:
					return OfflineWindowCloseReason.QueryShutdown;
				case DialogResult.OK:
					return OfflineWindowCloseReason.SubmitWorks;
				case DialogResult.Cancel:
					return OfflineWindowCloseReason.CancelWorks;
				case DialogResult.Abort:
					return OfflineWindowCloseReason.RequestStop;
				default:
					throw new ArgumentOutOfRangeException(nameof(result), result, null);
			}
		}

		private void pbMerge_MouseEnter(object sender, EventArgs e)
		{
			pbMerge.Image = Resources.merge_hover;
		}

		private void pbMerge_MouseLeave(object sender, EventArgs e)
		{
			pbMerge.Image = Resources.merge;
		}

		private void pbSplit_MouseEnter(object sender, EventArgs e)
		{
			pbSplit.Image = Resources.split_hover;
		}

		private void pbSplit_MouseLeave(object sender, EventArgs e)
		{
			pbSplit.Image = Resources.split;
		}

		private void pbSplit_Click(object sender, EventArgs e)
		{
			Presenter.SplitInterval();
		}

		private void pbMerge_Click(object sender, EventArgs e)
		{
			Presenter.MergeInterval();
		}

		private void OfflineWorkForm_SizeChanged(object sender, EventArgs e)
		{
			UpdateScrollbar();
		}

		private void OfflineWorkForm_LocationChanged(object sender, EventArgs e)
		{
			if (MinimumSize.Height != Height) return;
			var maxHeight = (int)(Screen.FromControl(this).Bounds.Height * 0.7);
			if (Height <= maxHeight) return;
			MinimumSize = new Size(MinimumSize.Width, maxHeight);
			Height = maxHeight;
			UpdateScrollbar();
		}
	}
}
