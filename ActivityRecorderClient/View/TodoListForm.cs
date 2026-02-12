using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.TodoLists;
using Tct.ActivityRecorderClient.View.Controls;
using Message = System.Windows.Forms.Message;

namespace Tct.ActivityRecorderClient.View
{
	public partial class TodoListForm : FixedMetroForm, IMessageFilter, ILocalizableControl
	{
		private readonly List<TodoListItemUserControl> listItemUserControls;
		private bool isModifiedState = false;
		private readonly TodoListsService todoListsService;
		private bool initializing = true;
		private bool pickerDropdown;
		private bool canDatePickerValueChanged = true;
		private DateTime previouslySetDate;
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		enum TokenState
		{
			None = 0,
			Acquiring = 1,
			Acquired = 2
		}

		private TokenState tokenState = TokenState.None;
		private bool shouldRefreshList = false;
		private Dragger dragger;
		private bool autoScrollUp, autoScrollDown;

		public bool ShouldAcquireToken { get; private set; } = true;

		public TodoListForm(TodoListsService service)
		{
			InitializeComponent();
			listItemUserControls = new List<TodoListItemUserControl> { todoListItemUserControl };
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			todoListsService = service;
			previouslySetDate = service.Today;
			panel.VerticalScroll.Enabled = true;
			panel.VerticalScroll.Visible = false;
			errorProvider.SetIconAlignment(okButton, ErrorIconAlignment.MiddleLeft);
		}

		protected new void BackgroundQuery<T>(Func<GeneralResult<T>> queryFunc, Action<T> onSuccess, string errorMessage)
		{
			BackgroundQuery(queryFunc, onSuccess, ex => DefaultBackgroundQueryOnError(ex, errorMessage));
		}

		protected new void DefaultBackgroundQueryOnError(Exception ex, string errorMessage)
		{
			var callSequenceInconsistencyException = ex as CallSequenceInconsistencyException;
			if (callSequenceInconsistencyException != null)
			{
				log.Warn("Error in WCF result order.");
				return;
			}
			base.DefaultBackgroundQueryOnError(ex, errorMessage);
		}

		private void addListElementButton_Click(object sender, EventArgs e)
		{
			TodoListItemUserControl listItemUC = new TodoListItemUserControl();
			listItemUC.KeyDown += keyDownHandler;
			listItemUserControls.Add(listItemUC);
			contentFlowLayoutPanel.SuspendLayout();
			contentFlowLayoutPanel.Controls.Add(listItemUC);
			listItemUC.Value = contentFlowLayoutPanel.Controls.Count - 1;
			contentFlowLayoutPanel.Controls.SetChildIndex(listItemUC, listItemUC.Value);
			contentFlowLayoutPanel.ResumeLayout();
			listItemUC.Focus();
			listItemUC.OnDelete += deleteListItem;
			listItemUC.OnChange += userControlContentModifiedHandler;
			listItemUC.DragEnter += HandleDragEntered;
			listItemUC.DragDrop += HandleDragDrop;
			listItemUC.DragOver += HandleDragMoved;
			userControlContentModifiedHandler("");
			previouslySetDate = dtpDay.Value.Date;
			RefreshScroll();
			ScrollTo(scrollBar.ScrollTotalSize - scrollBar.ScrollVisibleSize);
		}

		private void userControlContentModifiedHandler(string newContent)
		{
			contentModified();
			setErrorOnOkButton();
		}

		private void setErrorOnOkButton()
		{
			var hasListEmptyItem = listItemUserControls.Any(x => string.IsNullOrEmpty(x.Content));
			errorProvider.SetError(okButton, hasListEmptyItem ? Labels.TODOs_EmptyListItemError : "");
			okButton.Enabled = !hasListEmptyItem;
		}

		private void contentModified()
		{
			if (!isModifiedState)
			{
				isModifiedState = true;
				cancelButton.Text = Labels.Cancel;
				if (tokenState == TokenState.None && todoListsService.LastRecentDate == todoListsService.Today)
				{
					RefreshTokenholdersName(null);
					tokenState = TokenState.Acquiring;
					ThreadPool.QueueUserWorkItem(_ =>
					{
						var token = todoListsService.AcquireTodoListToken(dtpDay.Value, todoListsService.MostRecentTodoList.Id);
						if (token != null)
						{
							tokenAcquired();
						}
					});

				}
				else
				{
					okButton.Visible = true;
				}
			}
			setErrorOnOkButton();
		}

		public void RefreshTokenholdersName(TodoListToken token)
		{
			if (editedByMetroLabel.InvokeRequired)
			{
				editedByMetroLabel.Invoke(new Action(() => RefreshTokenholdersName(token)));
				return;
			}

			if (token != null)
			{
				foreach (var listItemUserControl in listItemUserControls)
				{
					listItemUserControl.Enabled = false;
				}
				addListElementButton.Enabled = false;
				shouldRefreshList = true;
			}
			metroProgressBar.Visible = true;
			editedByMetroLabel.Text = string.Format(Labels.TODOs_EditedBy, token?.EditedByLastName + " " + token?.EditedByFirstName);
			editedByMetroLabel.Visible = true;
		}


		private void tokenAcquired()
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => tokenAcquired()));
				return;
			}
			if (shouldRefreshList)
			{
				BackgroundQuery(() => todoListsService.GetList(dtpDay.Value.Date), x => drawList(x),
					Labels.Worktime_NoResponse);
				shouldRefreshList = false;
			}
			foreach (var listItemUserControl in listItemUserControls)
			{
				listItemUserControl.Enabled = true;
			}
			addListElementButton.Enabled = true;
			tokenState = TokenState.Acquired;
			editedByMetroLabel.Text = Labels.TODOs_Modifiable;
			metroProgressBar.Visible = false;
			okButton.TabStop = false;
			okButton.Visible = true;
			okButton.TabStop = true;
		}

		private void contentSaved()
		{
			if (isModifiedState)
			{
				if (isSaveMandatory)
					resetControlsAfterMandatorySave();
				okButton.Visible = false;
				cancelButton.Text = Labels.Close;
				isModifiedState = false;
				metroProgressBar.Visible = false;
				editedByMetroLabel.Visible = false;
				cancelTokenAcquiruing();
			}
		}

		private void TodoListForm_Load(object sender, EventArgs e)
		{
			var size = IsolatedStorageTodoListHelper.Size;
			if (size.HasValue) Size = size.Value;
			else log.Debug("Couldn't load TosoListForm's size.");
			metroProgressBar.TabStop = false;
			Localize();
			//RefreshScroll();
			Application.AddMessageFilter(this);
			Closed += TodoListForm_Closed;
		}

		public void Localize()
		{
			Text = Labels.TODOs;
			cancelButton.Text = isModifiedState ? Labels.Cancel : Labels.Close;
			okButton.Text = Labels.TODOs_Save;
			addListElementButton.Text = Labels.TODOs_Add;
		}

		private void TodoListForm_Closed(object sender, EventArgs e)
		{
			log.Debug("TodoList closed. (close)");
			Application.RemoveMessageFilter(this);
		}

		private void RemoveHandlersFromControl(TodoListItemUserControl control)
		{
			if (control == null) return;
			control.KeyDown -= keyDownHandler;
			control.OnDelete -= deleteListItem;
			control.OnChange -= userControlContentModifiedHandler;
			control.DragEnter -= HandleDragEntered;
			control.DragDrop -= HandleDragDrop;
			control.DragOver -= HandleDragMoved;
			control.PrepareForRemove();
		}

		private void deleteListItem(TodoListItemUserControl tliuc)
		{
			if (listItemUserControls.Contains(tliuc))
				listItemUserControls.Remove(tliuc);
			contentFlowLayoutPanel.Controls.Remove(tliuc);
			if (tliuc == lastDragOverControl) lastDragOverControl = null;
			contentModified();
			RefreshScroll();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			saveTodoList(dtpDay.Value.Date);
		}

		private void saveTodoList(DateTime date)
		{
			log.Debug("Saving todolist...");
			IsolatedStorageTodoListHelper.Save(Size);
			if (listItemUserControls.Any(x => string.IsNullOrEmpty(x.Content)))
			{
				log.Debug("There is at least 1 empty item in the list. Cancel saving.");
				MessageBox.Show(Labels.TODOs_EmptyListItemError, Labels.Error);
				return;
			}

			Hide();
			var todoListToSave = listItemUserControls.Select(
					x => new TodoListViewObject(x.Id, x.State, x.Content, listItemUserControls.IndexOf(x), x.CreatedAt ?? DateTime.Today
				)).ToList();
			BackgroundQuery(
				() =>
				{
					var res = todoListsService.SaveTodoLists(todoListToSave, date);
					return res;
				},
				x =>
				{
					contentSaved();
					changeDate();
				},
				y =>
				{
					if (y.Message == TodoListsService.ListAlreadySavedMessage)
					{
						BringFront();
						if (MessageBox.Show(this, Labels.TODOs_ListAlreadySaved, Labels.Error, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.OK)
						{
							contentSaved();
							Close();
							return;
						} else
						{
							BringFront();
							return;
						}
					}
					BringFront();
					DefaultBackgroundQueryOnError(y, Labels.Worktime_NoResponse);

				}
				);
		}

		private void cancelTokenAcquiruing()
		{
			if (tokenState == TokenState.None) return;
			metroProgressBar.Visible = false;
			editedByMetroLabel.Visible = false;
			addListElementButton.Enabled = true;
			tokenState = TokenState.None;
			shouldRefreshList = false;
			ThreadPool.QueueUserWorkItem(_ => todoListsService.ReleaseTodoListToken());
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			if (tokenState == TokenState.Acquiring)
			{
				cancelTokenAcquiruing();
				contentSaved();
				BackgroundQuery(() => todoListsService.GetList(dtpDay.Value.Date), x => drawList(x),
					Labels.Worktime_NoResponse);
				return;
			}
			if (!isModifiedState)
			{
				Close();
			}
			else
			{
				log.Debug("Canceling todoList modifications.");
				contentSaved();
				changeDate();
			}
		}

		private void btnPrev_Click(object sender, EventArgs e)
		{
			dtpDay.Value = dtpDay.Value.AddDays(-1);
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			dtpDay.Value = dtpDay.Value.AddDays(1);
		}

		private void dtpDay_ValueChanged(object sender, EventArgs e)
		{
			if (initializing | pickerDropdown | !canDatePickerValueChanged) return;
			changeDate();
		}

		private void changeDate()
		{
			if (tokenState == TokenState.Acquiring)
			{
				cancelTokenAcquiruing();
				contentSaved();
			}

			if (isModifiedState)
			{
				var res = MessageBox.Show(Labels.TODOs_PromptSaveChanges, Application.ProductName, MessageBoxButtons.YesNoCancel);
				switch (res)
				{
					case DialogResult.Yes:
						canDatePickerValueChanged = false;
						var tmp = dtpDay.Value;
						dtpDay.Value = previouslySetDate;
						saveTodoList(previouslySetDate);
						dtpDay.Value = tmp;
						canDatePickerValueChanged = true;
						contentSaved();
						break;
					case DialogResult.No:
						contentSaved();
						break;
					case DialogResult.Cancel:
						canDatePickerValueChanged = false;
						dtpDay.Value = previouslySetDate;
						canDatePickerValueChanged = true;
						return;
				}
			}
			previouslySetDate = dtpDay.Value.Date;
			addListElementButton.Visible = dtpDay.Value.Date == todoListsService.Today;
			BackgroundQuery(() => todoListsService.GetList(dtpDay.Value.Date), x => drawList(x),
				Labels.Worktime_NoResponse);
		}

		protected override void SetBusyImpl(bool isBusy)
		{
			dtpDay.Enabled = !isBusy;
			btnNext.Enabled = !isBusy;
			btnPrev.Enabled = !isBusy;
			okButton.Enabled = !isBusy;
			metroProgressBar.Visible = isBusy;
			foreach (Control control in contentFlowLayoutPanel.Controls)
			{
				control.Enabled = !isBusy;
			}
		}

		public new void Focus()
		{

		}

		private void drawList(IEnumerable<TodoListViewObject> todoList)
		{
			bool isDateToday = dtpDay.Value.Date == todoListsService.Today;
			listItemUserControls.Clear();
			if (isDateToday) addListElementButton.Visible = true;
			if (todoList == null)
			{
				contentFlowLayoutPanel.Controls.Clear();
				RefreshScroll();
				return;
			}

			var listType = isDateToday
				? TodoListItemUserControl.ListType.IsContentModifiable | TodoListItemUserControl.ListType.IsStatusModifiable
				: (dtpDay.Value.Date == todoListsService.LastRecentDate
					? TodoListItemUserControl.ListType.IsStatusModifiable
					: TodoListItemUserControl.ListType.None);
			List<Control> controls = new List<Control>();
			TodoListItemUserControl lastTodoListItemUserControl = null;
			foreach (var el in todoList)
			{
				var listItemUC = new TodoListItemUserControl(el, listType);
				listItemUC.CreatedAt = el.CreatedAt;
				listItemUC.KeyDown += keyDownHandler;
				listItemUserControls.Add(listItemUC);
				listItemUC.Width = contentFlowLayoutPanel.ClientSize.Width;
				controls.Add(listItemUC);
				listItemUC.OnDelete += deleteListItem;
				listItemUC.OnChange += userControlContentModifiedHandler;
				listItemUC.DragEnter += HandleDragEntered;
				listItemUC.DragDrop += HandleDragDrop;
				listItemUC.DragOver += HandleDragMoved;
				lastTodoListItemUserControl = listItemUC;
			}
			contentFlowLayoutPanel.SuspendLayout();
			contentFlowLayoutPanel.Controls.Clear();
			contentFlowLayoutPanel.Controls.AddRange(controls.ToArray());
			contentFlowLayoutPanel.ResumeLayout(true);
			RefreshScroll();
			lastTodoListItemUserControl?.Focus();
			ActiveControl = lastTodoListItemUserControl;
			ScrollTo(0);
			setErrorOnOkButton();
		}

		private bool isSaveMandatory = false;

		public void ShowForMandatorySave(IEnumerable<TodoListViewObject> list)
		{
			if (!isSaveMandatory)
			{
				contentSaved();
				drawList(list);
				dtpDay.Value = todoListsService.Today;
				dtpDay.Enabled = false;
				contentModified();
				okButton.Visible = true;
				cancelButton.Visible = false;
				ControlBox = false;
				btnNext.Enabled = false;
				btnPrev.Enabled = false;
				isSaveMandatory = true;
				BringFront();
				ActiveControl = null;
			}
			else
			{
				BringFront();
			}
		}

		private void connectionError(Exception e)
		{
			Text = Labels.TODOs + " - " + Labels.NewWork_NoConnection;
			log.Error("Something went wrong in showing the todolist.", e);
			BackgroundQuery(() =>
				{
					Thread.Sleep(1000);
					return todoListsService.GetList(todoListsService.Today);
				},
				x =>
				{
					Text = Labels.TODOs;
					drawList(x);
					if (!isSaveMandatory)
					{
						dtpDay.Value = todoListsService.Today;
						dtpDay.Enabled = false;
						contentModified();
						cancelButton.Visible = false;
						ControlBox = false;
						btnNext.Enabled = false;
						btnPrev.Enabled = false;
						isSaveMandatory = true;
						BringFront();
					}
				},
				y => connectionError(y));
		}

		public void BringFront()
		{
			if (!Visible) Show();
			if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
			TopMost = true;
			Activate();
			TopMost = false;
		}

		private void resetControlsAfterMandatorySave()
		{
			cancelButton.Visible = true;
			ControlBox = true;
			btnNext.Enabled = true;
			btnPrev.Enabled = true;
			dtpDay.Enabled = true;
			isSaveMandatory = false;
		}

		private void dtpDay_DropDown(object sender, EventArgs e)
		{
			pickerDropdown = true;
		}

		private void dtpDay_CloseUp(object sender, EventArgs e)
		{
			pickerDropdown = false;
			changeDate();
		}

		private void HandleScrolled(object sender, EventArgs e)
		{
			ScrollTo(scrollBar.Value);
		}

		private void ScrollTo(int verticalPosition)
		{
			if (verticalPosition < 0) verticalPosition = 0;
			if (verticalPosition > scrollBar.ScrollTotalSize - scrollBar.ScrollVisibleSize) verticalPosition = scrollBar.ScrollTotalSize - scrollBar.ScrollVisibleSize;
			scrollBar.Value = verticalPosition;
			if (verticalPosition < 0)
				contentFlowLayoutPanel.Location = new Point(0, 0);
			else
				contentFlowLayoutPanel.Location = new Point(0, -verticalPosition);
		}

		private void ScrollTo(TodoListItemUserControl liuc)
		{
			if (scrollBar.Value < liuc.Location.Y + liuc.Size.Height * 2 - scrollBar.Height)
				ScrollTo(liuc.Location.Y + liuc.Size.Height * 2 - scrollBar.Height);
			if (scrollBar.Value > liuc.Location.Y - liuc.Size.Height * 2)
				ScrollTo(liuc.Location.Y - liuc.Size.Height * 2);
		}

		private void RefreshScroll()
		{
			contentFlowLayoutPanel.Height = contentFlowLayoutPanel.PreferredSize.Height;
			scrollBar.ScrollTotalSize = contentFlowLayoutPanel.Height;
			scrollBar.ScrollVisibleSize = panel.Height;
			ScrollTo(scrollBar.Value);
		}

		private bool CheckCursorIsInsideWindow()
		{
			var hwnd = WinApi.WindowFromPoint(Cursor.Position);
			var c = Control.FromHandle(hwnd);
			if (c == null) return false;
			while (c.Parent != null) c = c.Parent;
			return Handle == c.Handle;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (!Visible) return false;
			if (m.Msg == (int)WinApi.Messages.WM_MOUSEWHEEL && Bounds.Contains(Cursor.Position))
			{
				if (!CheckCursorIsInsideWindow()) return false;
				var scrollDelta = (short)(((long)m.WParam >> 16) & 0xffff);
				scrollDelta = (short)((scrollDelta < 0 ? scrollDelta - 2 : scrollDelta + 2) / 3); //take one third of the original value
				scrollBar.ScrollDelta(scrollDelta);
				return true;
			}
			return false;
		}

		public void StartDrag(Control row, Point? startPosition = null)
		{
			log.Debug("UI - Trying to drag element");
			log.Debug("UI - Dragging element");
			TelemetryHelper.RecordFeature("TodoList", "Drag");
			dragger = new Dragger(row, startPosition);
			row.Visible = false;
			Cursor.Clip = RectangleToScreen(ClientRectangle);
			DragDropEffects res =
				DoDragDrop(
					new DragContainer<int> { ParentName = Name, Content = contentFlowLayoutPanel.Controls.IndexOf(row) },
					DragDropEffects.Move | DragDropEffects.Copy);
			if (res == DragDropEffects.None || res == DragDropEffects.Copy)
			{
				row.Visible = true;
			}

			if (lastDragOverControl != null)
			{
				lastDragOverControl.Margin = TodoListItemUserControl.DefaultMargin;
				lastDragOverControl = null;
			}

			dragger.Dispose();
			dragger = null;
			Cursor.Clip = new Rectangle();
			log.Debug("UI - Element dragged");
		}

		private void keyDownHandler(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Down:
					focusNextUserControl();
					break;
				case Keys.Up:
					focusPrevUserControl();
					break;
				case Keys.Enter:
					enterPressed();
					break;
				case Keys.Back:
					backSpacePressed();
					break;
				case Keys.S:
					if (WinApi.GetKeyState(VK_CONTROL) >> 8 != 0 && isModifiedState)
					{
						okButton_Click(null, null);
					}
					break;
				case Keys.Escape:
					if (isModifiedState)
					{
						findAndDeleteCurrentListItem();
						break;
					}
					Close();
					break;
			}
		}

		private void findAndDeleteCurrentListItem()
		{
			int i = listItemUserControls.Count;
			while (i-- > 0)
			{
				if (listItemUserControls[i].ActiveControl != null)
				{
					if (!listItemUserControls[i].GetListType.HasFlag(TodoListItemUserControl.ListType.IsDeletable))
						return;
					deleteListItem(listItemUserControls[i]);
					if (listItemUserControls.Count > 0)
					{
						if (i - 1 >= 0)
						{
							var liuc = listItemUserControls[i - 1];
							liuc.Focus();
							ScrollTo(liuc);
						}
						else
						{
							var liuc = listItemUserControls[i];
							liuc.Focus();
							ScrollTo(liuc);
						}
					}
					setErrorOnOkButton();
					return;
				}
			}
		}

		private const int VK_UP = 0x26;
		private const int VK_DOWN = 0x28;
		private const int VK_RETURN = 0x0D;
		private const int VK_CONTROL = 0x11;
		private const int VK_ESCAPE = 0x1B;

		private void focusNextUserControl()
		{
			int i = 0;
			while (i < listItemUserControls.Count - 1)
			{
				if (listItemUserControls[i++].ActiveControl != null)
				{
					var liuc = listItemUserControls[i];
					liuc.Focus();
					ScrollTo(liuc);
					break;
				}
			}
		}

		private void focusPrevUserControl()
		{
			int i = listItemUserControls.Count - 1;
			while (i > 0)
			{
				if (listItemUserControls[i--].ActiveControl != null)
				{
					var liuc = listItemUserControls[i];
					liuc.Focus();
					ScrollTo(liuc);
					break;
				}
			}
		}

		private void enterPressed()
		{
			for (int i = 0; i < listItemUserControls.Count; i++)
			{
				if (listItemUserControls[i].ActiveControl != null)
				{
					var listItem = insertEmptyListItem(i + 1);
					ScrollTo(listItem);
					return;
				}
			}
			if (listItemUserControls.Count == 0
				&& addListElementButton.Enabled
				&& addListElementButton.Visible)
			{
				addListElementButton.PerformClick();
			}
		}

		private void backSpacePressed()
		{
			for (int i = 0; i < listItemUserControls.Count; i++)
			{
				var listItem = listItemUserControls[i];
				if (listItem.ActiveControl != null && string.IsNullOrEmpty(listItem.Content))
				{
					deleteListItem(listItem);
					if (i > 0)
					{
						var currentListItem = listItemUserControls[i - 1];
						currentListItem.FocusEnd();
						ScrollTo(currentListItem);
					}
					return;
				}
			}
		}

		private TodoListItemUserControl insertEmptyListItem(int index)
		{
			TodoListItemUserControl listItemUC = new TodoListItemUserControl();
			listItemUC.KeyDown += keyDownHandler;
			listItemUserControls.Insert(index, listItemUC);
			contentFlowLayoutPanel.SuspendLayout();
			contentFlowLayoutPanel.Controls.Add(listItemUC);
			contentFlowLayoutPanel.Controls.SetChildIndex(listItemUC, index);
			listItemUC.Value = index;
			contentFlowLayoutPanel.ResumeLayout();
			listItemUC.Focus();
			listItemUC.OnDelete += deleteListItem;
			listItemUC.OnChange += userControlContentModifiedHandler;
			listItemUC.DragEnter += HandleDragEntered;
			listItemUC.DragDrop += HandleDragDrop;
			listItemUC.DragOver += HandleDragMoved;
			userControlContentModifiedHandler("");
			previouslySetDate = dtpDay.Value.Date;
			RefreshScroll();
			ScrollTo(scrollBar.ScrollTotalSize - scrollBar.ScrollVisibleSize);
			return listItemUC;
		}

		private void TodoListForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			log.Debug("TodoList closing (hide).");
			//just in case
			Cursor.Clip = new Rectangle();
			ThreadPool.QueueUserWorkItem(_ => { IsolatedStorageTodoListHelper.Save(Size); });
			if (isModifiedState)
			{
				var res = MessageBox.Show(Labels.TODOs_PromptSaveChanges, Application.ProductName, MessageBoxButtons.YesNoCancel);
				switch (res)
				{
					case DialogResult.Yes:
						saveTodoList(dtpDay.Value.Date);
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}

			if (isSaveMandatory)
			{
				e.Cancel = true;
			}

			if (e.Cancel) return;
			if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown) return;
			e.Cancel = true;
			Hide();
		}

		private void contentFlowLayoutPanel_SizeChanged(object sender, EventArgs e)
		{
			contentFlowLayoutPanel.SuspendLayout();
			foreach (Control control in contentFlowLayoutPanel.Controls)
			{
				if (control is TodoListItemUserControl)
				{
					control.Width = contentFlowLayoutPanel.ClientSize.Width;
				}
			}
			RefreshScroll();
			contentFlowLayoutPanel.ResumeLayout();
		}

		private void HandleDragging(object sender, GiveFeedbackEventArgs e)
		{
			dragger.UpdateCursor(sender, e);
		}

		private TodoListItemUserControl lastDragOverControl;
		private void HandleDragEntered(object sender, DragEventArgs e)
		{
			var dragData = e.Data.GetData(typeof(DragContainer<int>)) as DragContainer<int>;
			if (dragData != null && dragData.ParentName == Name)
			{
				e.Effect = DragDropEffects.Move;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void HandleDragMoved(object sender, DragEventArgs e)
		{
			var lastCtrl = contentFlowLayoutPanel.Controls.OfType<TodoListItemUserControl>().LastOrDefault(c => c.Visible);
			if (lastCtrl != null && e.Y >= lastCtrl.PointToScreen(Point.Empty).Y + lastCtrl.Height / 2 && lastDragOverControl == lastCtrl)
			{
				lastDragOverControl.Margin = TodoListItemUserControl.DefaultMargin;
				lastDragOverControl = null;
				return;
			}

			var overCtrl = contentFlowLayoutPanel.Controls
				.OfType<TodoListItemUserControl>()
				.FirstOrDefault(c =>
				{
					var y = c.PointToScreen(Point.Empty).Y;
					return c.Visible && (e.Y >= y - c.Height / 2 && e.Y < y + c.Height / 2
										 || e.Y < y - c.Height / 2 && e.Y >= y - 3 * c.Height / 2);
				});
			if (overCtrl == null) return;
			if (e.Y < panel.PointToScreen(Point.Empty).Y + overCtrl.Height / 2)
			{
				if (!autoScrollTimer.Enabled)
				{
					autoScrollUp = true;
					autoScrollTimer.Start();
				}
			}
			else autoScrollUp = false;

			if (e.Y > panel.PointToScreen(Point.Empty).Y + panel.Height - overCtrl.Height / 2)
			{
				if (!autoScrollTimer.Enabled)
				{
					autoScrollDown = true;
					autoScrollTimer.Start();
				}
			}
			else autoScrollDown = false;

			if (lastDragOverControl == overCtrl) return;
			contentFlowLayoutPanel.SuspendLayout();
			if (lastDragOverControl != null) lastDragOverControl.Margin = TodoListItemUserControl.DefaultMargin;
			overCtrl.Margin = new Padding(overCtrl.Margin.Left, overCtrl.Margin.Top * 2 + overCtrl.Margin.Bottom + overCtrl.Height, overCtrl.Margin.Right, overCtrl.Margin.Bottom);
			lastDragOverControl = overCtrl;
			contentFlowLayoutPanel.ResumeLayout(true);
		}

		private void HandleDragDrop(object sender, DragEventArgs e)
		{
			log.Debug("Drag dropped");
			var dragData = e.Data.GetData(typeof(DragContainer<int>)) as DragContainer<int>;
			if (dragData == null) return;
			if (dragData.ParentName != Name) return;
			int dragTargetIndex =
				contentFlowLayoutPanel.Controls.Cast<Control>()
					.Where(x => x is ISelectable<int> && x.Visible)
					.TakeWhile(ctrl => e.Y >= ctrl.PointToScreen(Point.Empty).Y + ctrl.Height / 2)
					.Count();
			var draggedCtrl = contentFlowLayoutPanel.Controls[dragData.Content] as TodoListItemUserControl;
			Debug.Assert(draggedCtrl != null);
			contentFlowLayoutPanel.SuspendLayout();
			draggedCtrl.Visible = true;
			if (lastDragOverControl != null)
			{
				lastDragOverControl.Margin = TodoListItemUserControl.DefaultMargin;
				lastDragOverControl = null;
			}
			Cursor = DefaultCursor;
			if (dragTargetIndex != dragData.Content)
			{
				contentModified();
				contentFlowLayoutPanel.Controls.SetChildIndex(draggedCtrl, dragTargetIndex);
				listItemUserControls.Remove(draggedCtrl);
				listItemUserControls.Insert(dragTargetIndex, draggedCtrl);
			}
			contentFlowLayoutPanel.ResumeLayout(true);
			TelemetryHelper.RecordFeature("TodoList", "Drop");
		}

		private void contentFlowLayoutPanel_ControlRemoved(object sender, ControlEventArgs e)
		{
			if (!(e.Control is TodoListItemUserControl control)) return;
			RemoveHandlersFromControl(control);
		}

		private void TodoListForm_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
				log.Debug("TodoList shown.");
				initializing = false;
				MaximizeBox = true; // https://stackoverflow.com/a/3304728/2295648
				if (!isSaveMandatory)
					BackgroundQuery(() => todoListsService.GetList(todoListsService.Today), x => drawList(x),
						y => { connectionError(y); });
			}
		}

		private void autoScrollTimer_Tick(object sender, EventArgs e)
		{
			if (dragger == null)
			{
				autoScrollTimer.Stop();
				return;
			}
			if (autoScrollUp) ScrollTo(-contentFlowLayoutPanel.Top - 10);
			else if (autoScrollDown) ScrollTo(-contentFlowLayoutPanel.Top + 10);
			else autoScrollTimer.Stop();
		}
	}
}
