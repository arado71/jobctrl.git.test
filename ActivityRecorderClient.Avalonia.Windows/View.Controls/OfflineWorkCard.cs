using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class OfflineWorkCard : UserControl, ILocalizableControl
	{
		private Dictionary<string, Action<MeetingInfo>> fieldActions;

		[Category("Appearance")]
		[Description("The background color of the component")]
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		public new Color BackColor { get { return pnlRound.BackColor; } set { pnlRound.BackColor = value; pnlRound.Invalidate(); } }

		private readonly MeetingInfo meetingInfo;
		private readonly SynchronizationContext context;
		private CardStyle cardStyle;
		private Color foreColor;
		private bool isAnyDataChanged;
		private bool pbParticipantsVisible;
		private bool skipTaskSelection;
		private PictureBox pbBadge;

		public event EventHandler Validation;
		public event EventHandler<SingleValueEventArgs<object>> WorkSelectionChanged;
		public event EventHandler<SingleValueEventArgs<bool>> CardExpandStateChanged;
		public event EventHandler<SingleValueEventArgs<bool>> CardDeletedStateChanged;
		public event EventHandler DataChanged;
		public event EventHandler AddressbookSelected;

		public OfflineWorkCard()
		{
			InitializeComponent();
			pnlRound.Border = 0;
			Height -= pnlEditor.Height;
			foreColor = lblDuration.ForeColor;
			tlpMain.BackColor = Color.Transparent;
			pbParticipantsVisible = true;
		}

		public OfflineWorkCard(SynchronizationContext context, MeetingInfo meetingInfo, Func<WorkData, bool> canSelectWork, Func<ClientMenuLookup, IEnumerable<int>> recentWorkIdsSelector) : this()
		{
			fieldActions = new Dictionary<string, Action<MeetingInfo>>
			{
				{nameof(meetingInfo.OfflineWorkType), SetOfflineWorkType           },
				{nameof(meetingInfo.DurationText), SetDurationText                 },
				{nameof(meetingInfo.StartTimeText), SetStartTimeText               },
				{nameof(meetingInfo.EndTimeText), SetEndTimeText                   },
				{nameof(meetingInfo.Subject), SetSubject                           },
				{nameof(meetingInfo.Comment), SetComment                           },
				{nameof(meetingInfo.Participants), SetParticipants                 },
				{nameof(meetingInfo.NavigationWork), SetNavigationWork             },
				{nameof(meetingInfo.ClientMenuLookup), SetClientMenuLookup         },
				{nameof(meetingInfo.IsTaskSelectionInvalid), SetTaskSelectionError },
				{nameof(meetingInfo.IsSubjectLengthInvalid), SetSubjectError       },
				{nameof(meetingInfo.IsDescriptionLengthInvalid), SetCommentError   },
				{nameof(meetingInfo.IsEmailsLengthInvalid), SetParticipantError    },
				{nameof(meetingInfo.IsEmailFormatError), SetParticipantError       },
				{nameof(meetingInfo.IsDurationInvalid), SetDurationError           },
				{nameof(meetingInfo.IsDurationExceedLimit), SetDurationError       },
				{nameof(meetingInfo.CardStyle), SetCardStyle                       },
				{nameof(meetingInfo.IsAddressbookVisible), SetPhonebookVisible     },
				{nameof(meetingInfo.IsWaitCursorOnAddressbook), SetWaitcursorOnAddressbook},
				{nameof(meetingInfo.HasBadge), SetBadgeVisible                     },
			};

			this.context = context;
			this.meetingInfo = meetingInfo;
			Tag = meetingInfo.Id;
			cbTask.CanSelectWork = canSelectWork;
			cbTask.RecentWorkIdsSelector = recentWorkIdsSelector;
			meetingInfo.PropertyChanged += MeetingInfoPropertyChanged;
			//TODO: should unload handler when disposed?
			txbSubject.AutoCompleteCustomSource.AddRange(Meeting.AddMeetingHistory.RecentSubjects.ToArray());
			txbParticipants.RecentTags = Meeting.AddMeetingHistory.RecentEmails;
			Localize();
		}

		public void Localize()
		{
			lblSubject.Text = Labels.AddMeeting_Subject + @":";
			lblTask.Text = Labels.AddMeeting_SelectedWork + @":";
			lblParticipants.Text = Labels.AddMeeting_Participiants + @":";
			lblComment.Text = Labels.AddMeeting_Description + @":";
			lblUndo.Text = Labels.AddMeeting_UndoDeleted;
			lblDeletedInfo.Text = Labels.AddMeeting_DeletedInfo;
			lblChooseTask.Text = Labels.AddMeeting_ChooseTask;
			toolTip.SetToolTip(pbParticipants, Labels.AddMeeting_AddressBook);
		}

		public void InitFieldValues()
		{
			SuspendLayout();
			foreach (var action in fieldActions.Values)
			{
				action(meetingInfo);
			}
			ResumeLayout();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			meetingInfo.PropertyChanged -= MeetingInfoPropertyChanged;
		}

		public MeetingInfo MeetingInfo => meetingInfo;

		public void ActivateFirstUnfilled()
		{
			//if (meetingInfo.NavigationWork == null)
				cbTask.Select();
			//else
			//{
			//	txbSubject.Select();
			//	if (txbSubject.Text.Length > 0)
			//		txbSubject.SelectAll();
			//}
		}

		public void DropdownTaskList(bool isDrop = true)
		{
			cbTask.DroppedDown = isDrop;
		}

		void MeetingInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// changes should become from GUI thread
			Action<MeetingInfo> action;
			if (fieldActions.TryGetValue(e.PropertyName, out action))
				action(meetingInfo);
		}

		private void SetCardStyle(MeetingInfo info)
		{
			if (cardStyle == info.CardStyle) return;
			//SuspendLayout();
			var visible = info.CardStyle == CardStyle.Selected;
			if (visible && cardStyle != CardStyle.Selected || info.CardStyle != CardStyle.Selected && cardStyle == CardStyle.Selected)
			{
				SetEditPanelVisible(visible);
				CardExpandStateChanged?.Invoke(this, SingleValueEventArgs.Create(visible));
				skipTaskSelection = true;
				if (visible)
					ActivateFirstUnfilled();
				else
					lblSubject.Select(); //dummy
				skipTaskSelection = false;
			}
			cardStyle = info.CardStyle;
			switch (info.CardStyle)
			{
				case CardStyle.Normal:
					pnlRound.Border = 0;
					BackColor = Color.FromKnownColor(KnownColor.Control);
					lblDuration.ForeColor = foreColor;
					lblFrom.ForeColor = foreColor;
					lblInterval.ForeColor = foreColor;
					lblTo.ForeColor = foreColor;
					SetWorkinfoPanelState(info.NavigationWork != null);
					pbArrow.Image = Resources.arrow_grey_right;
					pbArrow.Visible = true;
					break;
				case CardStyle.Selected:
					pnlRound.BorderColor = Color.CornflowerBlue;
					pnlRound.Border = 2;
					BackColor = Color.FromKnownColor(KnownColor.Control);
					lblDuration.ForeColor = foreColor;
					lblFrom.ForeColor = foreColor;
					lblInterval.ForeColor = foreColor;
					lblTo.ForeColor = foreColor;
					SetWorkinfoPanelState(info.NavigationWork != null);
					pbArrow.Image = Resources.arrow_blue;
					pbArrow.Visible = true;
					break;
				case CardStyle.Deleted:
					pnlRound.BorderColor = Color.Silver;
					pnlRound.Border = 2;
					BackColor = Color.White;
					lblDuration.ForeColor = Color.Silver;
					lblFrom.ForeColor = Color.Silver;
					lblInterval.ForeColor = Color.Silver;
					lblTo.ForeColor = Color.Silver;
					SetWorkinfoPanelState(null);
					pbArrow.Visible = false;
					break;
				case CardStyle.Incomplete:
					pnlRound.BorderColor = Color.Red;
					pnlRound.Border = 2;
					BackColor = Color.FromKnownColor(KnownColor.Control);
					lblDuration.ForeColor = foreColor;
					lblFrom.ForeColor = foreColor;
					lblInterval.ForeColor = foreColor;
					lblTo.ForeColor = foreColor;
					SetWorkinfoPanelState(info.NavigationWork != null);
					pbArrow.Image = Resources.arrow_grey_right;
					pbArrow.Visible = true;
					break;
				case CardStyle.CannotAccountable:
					pnlRound.Border = 0;
					BackColor = Color.OrangeRed;
					lblDuration.ForeColor = Color.White;
					lblChooseTask.ForeColor = Color.White;
					SetWorkinfoPanelState(false);
					pbArrow.Visible = false;
					tlpMain.Controls.Remove(pbDelete);
					tlpMain.Controls.Remove(pnlSplit4);
					tlpMain.Controls.Remove(lblTo);
					tlpMain.Controls.Remove(lblInterval);
					tlpMain.Controls.Remove(lblFrom);
					tlpMain.Controls.Remove(pnlSplit2);
					tlpMain.Controls.Remove(pbType);
					var lblText = new Label
					{
						Text = Labels.AddMeeting_NotAccTitle,
						ForeColor = Color.White,
						Height = lblFrom.Height,
						Width = WidthWithMargins(pbType) + 2 + WidthWithMargins(lblFrom) + WidthWithMargins(lblInterval) + WidthWithMargins(lblTo) - 6,
						Dock = DockStyle.Fill,
						TextAlign = ContentAlignment.MiddleLeft,
						AutoEllipsis = true
					};
					pnlTask.Margin = new Padding(3, 0, 3, 0);
					lblChooseTask.Text = Labels.AddMeeting_NotAccText;
					tlpMain.Controls.Add(lblText, 3, 0);
					tlpMain.SetColumnSpan(pnlTask, 3);
					tlpMain.SetColumnSpan(lblText, 5);
					break;
				default:
					// ReSharper disable once NotResolvedInText
					throw new ArgumentOutOfRangeException("CardStyle", info.CardStyle, null);
			}
			//ResumeLayout(true);
		}

		private int WidthWithMargins(Control ctrl)
		{
			return ctrl.Width + ctrl.Margin.Left + ctrl.Margin.Right;
		}

		private void SetClientMenuLookup(MeetingInfo info)
		{
			if (info.ClientMenuLookup != null)
				cbTask.UpdateMenu(info.ClientMenuLookup);
		}

		private void SetNavigationWork(MeetingInfo info)
		{
			if (info.NavigationWork != null)
			{
				workInfo.UnselectedBackgroundColor = BackColor;
				workInfo.Navigation = meetingInfo.NavigationWork;
				SetWorkinfoPanelState(true);
				cbTask.SetSelectedItem(meetingInfo.NavigationWork?.Work);
			}
			else SetWorkinfoPanelState(false);
		}

		private void SetEndTimeText(MeetingInfo info)
		{
			if (info.EndTimeText == null) return;
			if (lblTo.Text != info.EndTimeText.Item1)
				lblTo.Text = info.EndTimeText.Item1;
			toolTip.SetToolTip(lblTo, info.EndTimeText.Item2);
		}

		private void SetStartTimeText(MeetingInfo info)
		{
			if (info.StartTimeText == null) return;
			if (lblFrom.Text != info.StartTimeText.Item1)
				lblFrom.Text = info.StartTimeText.Item1;
			toolTip.SetToolTip(lblFrom, info.StartTimeText.Item2);
		}

		private void SetDurationText(MeetingInfo info)
		{
			if (info.DurationText == null) return;
			if (lblDuration.Text != info.DurationText.Item1)
				lblDuration.Text = info.DurationText.Item1;
			toolTip.SetToolTip(lblDuration, info.DurationText.Item2);
		}

		private void SetSubject(MeetingInfo info)
		{
			if (info.Subject == null || info.Subject == txbSubject.Text) return;
			txbSubject.Text = info.Subject;
		}

		private void SetComment(MeetingInfo info)
		{
			if (info.Comment == null || info.Comment == txbComment.Text) return;
			txbComment.Text = info.Comment;
		}

		private void SetParticipants(MeetingInfo info)
		{
			if (info.Participants == null || info.Participants == txbParticipants.Text) return;
			txbParticipants.Text = info.Participants;
		}

		private void SetOfflineWorkType(MeetingInfo info)
		{
			pbType.Image = info.OfflineWorkType == OfflineWorkType.AfterInactivity ? Resources.idle : Resources.manual;
			toolTip.SetToolTip(pbType,
				info.OfflineWorkType == OfflineWorkType.AfterInactivity ? Labels.AddMeeting_Subtitle : Labels.AddMeeting_SubtitleHK);
		}

		private void SetTaskSelectionError(MeetingInfo info)
		{
			errorProvider.SetError(lblTask, info.IsTaskSelectionInvalid ? Regex.Replace(Labels.AddMeeting_SelectWorkCaption, "[.]$", "") + "!" : "");
		}

		private void SetSubjectError(MeetingInfo info)
		{
			errorProvider.SetError(lblSubject, info.IsSubjectLengthInvalid ? Labels.AddMeeting_SubjectLengthInvalid + "!" : "");
		}

		private void SetCommentError(MeetingInfo info)
		{
			errorProvider.SetError(lblComment, info.IsDescriptionLengthInvalid ? Labels.AddMeeting_DescriptionLengthInvalid + "!" : "");
		}

		private void SetParticipantError(MeetingInfo info)
		{
			errorProvider.SetError(lblParticipants, info.IsEmailsLengthInvalid ? Labels.AddMeeting_EmailsLengthInvalid : info.IsEmailFormatError ? Labels.AddMeeting_EmailFormatError + "!" : "");
		}

		private void SetDurationError(MeetingInfo info)
		{
			errorProvider.SetError(lblTo, info.IsDurationInvalid ? info.IsDurationExceedLimit ? Labels.AddMeeting_DurationInvalid : Labels.AddMeeting_ExceedLimit : "");
		}

		private void SetPhonebookVisible(MeetingInfo info)
		{
			if (pbParticipantsVisible == info.IsAddressbookVisible) return;
			pbParticipants.Visible = pbParticipantsVisible = info.IsAddressbookVisible;
			txbParticipants.Width = info.IsAddressbookVisible ? txbComment.Width - pbParticipants.Width : txbComment.Width;
		}

		private void SetWaitcursorOnAddressbook(MeetingInfo info)
		{
			if (pbParticipants.UseWaitCursor == info.IsWaitCursorOnAddressbook) return;
			pbParticipants.UseWaitCursor = info.IsWaitCursorOnAddressbook;
		}

		private void SetBadgeVisible(MeetingInfo info)
		{
			if (pbBadge == null && !info.HasBadge || pbBadge != null && pbBadge.Visible == info.HasBadge) return;
			if (pbBadge == null)
			{
				pbBadge = new PictureBox { Image = Resources.badge, SizeMode = PictureBoxSizeMode.AutoSize, Padding = new Padding(0), Margin = new Padding(0), Location = new Point(lblTo.Width - Resources.badge.Width, lblTo.Height / 2 - 8) };
				lblTo.BackColor = Color.Transparent;
				lblTo.Controls.Add(pbBadge);
			}
			pbBadge.Visible = info.HasBadge;
		}

		public event EventHandler CardClicked;

		private void HandleClicked(object sender, EventArgs e)
		{
			CardClicked?.Invoke(this, EventArgs.Empty);
		}

		private void SetEditPanelVisible(bool visible)
		{
			if (visible)
			{
				Height += pnlEditor.Height;
			}
			else
			{
				lblDuration.Select();
				Height -= pnlEditor.Height;
			}
			pnlEditor.Visible = visible;
		}

		/// <summary>
		/// Set Workinfo Panel State
		/// </summary>
		/// <param name="state">true: workinfo selector, false: no work notification, null: deleted worktime</param>
		private void SetWorkinfoPanelState(bool? state)
		{
			workInfo.Visible = state.HasValue && state.Value;
			lblChooseTask.Visible = state.HasValue && !state.Value;
			tlpDeletedInfo.Visible = !state.HasValue;
			pbDelete.Visible = state.HasValue;
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);
			workInfo.UnselectedBackgroundColor = BackColor;
		}

		private void pbDelete_Click(object sender, EventArgs e)
		{
			CardDeletedStateChanged?.Invoke(this, new SingleValueEventArgs<bool>(true));
		}

		private void cbTask_SelectionChangeCommitted(object sender, EventArgs e)
		{
			WorkSelectionChanged?.Invoke(this, SingleValueEventArgs.Create(cbTask.SelectedItem));
			DataChanged?.Invoke(this, EventArgs.Empty);
			Validation?.Invoke(this, EventArgs.Empty);
		}

		private void txbSubject_TextChanged(object sender, EventArgs e)
		{
			meetingInfo.Subject = txbSubject.Text;
			Validation?.Invoke(this, EventArgs.Empty);
			isAnyDataChanged = true;
		}

		private void txbComment_TextChanged(object sender, EventArgs e)
		{
			meetingInfo.Comment = txbComment.Text;
			Validation?.Invoke(this, EventArgs.Empty);
			isAnyDataChanged = true;
		}

		private void txbParticipants_TextChanged(object sender, EventArgs e)
		{
			meetingInfo.Participants = txbParticipants.Text;
			Validation?.Invoke(this, EventArgs.Empty);
			isAnyDataChanged = true;
		}

		private void pbUndo_Click(object sender, EventArgs e)
		{
			CardDeletedStateChanged?.Invoke(this, new SingleValueEventArgs<bool>(false));
		}

		private void pbUndo_MouseEnter(object sender, EventArgs e)
		{
			pbUndo.Image = Resources.undo_hover;
			lblUndo.ForeColor = Color.FromArgb(18,156,221);
		}

		private void pbUndo_MouseLeave(object sender, EventArgs e)
		{
			pbUndo.Image = Resources.undo;
			lblUndo.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
		}

		private void pbDelete_MouseEnter(object sender, EventArgs e)
		{
			pbDelete.Image = Resources.delete_hover;
		}

		private void pbDelete_MouseLeave(object sender, EventArgs e)
		{
			pbDelete.Image = Resources.delete;
		}

		private void cbTask_Enter(object sender, EventArgs e)
		{
			if (cbTask.Items.Count > 0 && cbTask.SelectedIndex < 0)
			{
				cbTask.SelectedIndex = 0;
				cbTask_SelectionChangeCommitted(this, EventArgs.Empty);
			}
		}

		private void cbTask_Leave(object sender, EventArgs e)
		{
			if (skipTaskSelection) return;
			cbTask_SelectionChangeCommitted(this, EventArgs.Empty);
			cbTask.Text = cbTask.SelectedItem != null ? WorkDataWithParentNames.DefaultSeparator + cbTask.SelectedItem : null; // workaround for wrong selected work title
			cbTask.CanSelectWork = cbTask.CanSelectWork; // hax to perform UpdateDropDown
		}

		private void TextBoxLeave(object sender, EventArgs e)
		{
			if (!isAnyDataChanged) return;
			DataChanged?.Invoke(this, EventArgs.Empty);
			isAnyDataChanged = false;
		}

		private void pbArrow_MouseEnter(object sender, EventArgs e)
		{
			if (meetingInfo.CardStyle == CardStyle.CannotAccountable || meetingInfo.CardStyle == CardStyle.Deleted || meetingInfo.CardStyle == CardStyle.Selected) return;
			pbArrow.Image = Resources.arrow_blue_right;
		}

		private void pbArrow_MouseLeave(object sender, EventArgs e)
		{
			if (meetingInfo.CardStyle == CardStyle.CannotAccountable || meetingInfo.CardStyle == CardStyle.Deleted || meetingInfo.CardStyle == CardStyle.Selected) return;
			pbArrow.Image = Resources.arrow_grey_right;
		}

		private void pbParticipants_Click(object sender, EventArgs e)
		{
			AddressbookSelected?.Invoke(this, EventArgs.Empty);
		}

		private void pbSearch_Click(object sender, EventArgs e)
		{
			cbTask.Text = string.Empty;
			cbTask.Select();
		}
	}

}
