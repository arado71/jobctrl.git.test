using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MetroFramework;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Controls;
using Tct.ActivityRecorderService.WorkManagement;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WorkDetailsForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private Point lblReasonLocation;
		private Point txtReasonLocation;
		private int txtReasonHeight;
		private bool isModified;
		private int? selectedReasonId;
		private WorkDetailsFormState state;
		private Dictionary<int, string> reasonTypeLookup = new Dictionary<int, string>();
		private readonly static Regex LfRegex = new Regex("(?!\r)\n");
		private readonly static Regex CrLfRegex = new Regex("\r\n");

		private bool hasPermission;
		private bool HasPermission
		{
			get { return hasPermission; }
			set
			{
				hasPermission = value;
				txtName.Enabled = value;
				dtpStartDate.Enabled = value;
				dtpEndDate.Enabled = value;
				dtpDuration.Enabled = value;
				txtPriority.Enabled = value;
				cbCategory.Enabled = value;
				txtDescription.ReadOnly = !value;
				RefreshOkButton();
			}
		}

		public WorkManagementService Service { get; set; }
		public WorkData StartWork { get; private set; }

		private List<Reason> reasons;
		public List<Reason> Reasons
		{
			get { return reasons; }
			set
			{
				reasons = value;
				RefreshReasonList();
			}
		}

		public string Reason
		{
			get { return txtReason.Text; }
			set { txtReason.Text = value; }
		}

		private WorkData workToModify;
		public WorkData WorkToModify
		{
			get { return workToModify; }
			set
			{
				workToModify = value;
				var canModify = workToModify == null || !workToModify.IsReadOnly;
				txtName.Text = workToModify == null ? "" : workToModify.Name;
				txtName.Enabled = canModify;
				dtpStartDate.Checked = workToModify != null && workToModify.StartDate != null;
				dtpStartDate.Enabled = canModify;
				dtpEndDate.Checked = workToModify != null && workToModify.EndDate != null;
				dtpEndDate.Enabled = canModify;
				txtPriority.Text = workToModify == null || !workToModify.Priority.HasValue ? "" : workToModify.Priority.ToString();
				txtPriority.Enabled = canModify;
				dtpDuration.Value = workToModify != null ? workToModify.TargetTotalWorkTime : null;
				dtpDuration.Enabled = canModify;
				txtDescription.Text = workToModify != null && workToModify.Description != null ? LfRegex.Replace(workToModify.Description, "\r\n") : string.Empty;
				txtDescription.ReadOnly = !canModify;

				if (workToModify != null)
				{
					log.DebugFormat("WorkDetails set for {0} ({1})", workToModify.Id, workToModify.Name);
					if (workToModify.CategoryId != null)
					{
						cbCategory.SelectedValue = workToModify.CategoryId;
					}
					else
					{
						if (cbCategory.Items.Count > 0)
							cbCategory.SelectedIndex = 0;
					}

					cbCategory.Enabled = canModify;
					if (workToModify.StartDate != null) dtpStartDate.Value = workToModify.StartDate.Value;
					if (workToModify.EndDate != null) dtpEndDate.Value = workToModify.EndDate.Value;
					RefreshProjectName();
				}
				else
				{
					log.Debug("NULL WorkData passed");
				}

				btnAddReason.Visible = workToModify != null;
				btnCloseWork.Visible = workToModify != null && canModify;
				cbStart.Visible = workToModify == null;
				cbProject.Enabled = workToModify == null;
			}
		}

		public WorkDetailsFormState State
		{
			get { return state; }
			set
			{
				state = value;
				RefreshOkButton();
				switch (state)
				{
					case WorkDetailsFormState.AddReason:
						btnCancel.Text = Labels.MenuBack;
						this.Text = Labels.AddReason;
						SwitchPanels(true);
						break;
					case WorkDetailsFormState.CloseWork:
						btnCancel.Text = Labels.MenuBack;
						this.Text = Labels.CloseWork;
						SwitchPanels(true);
						break;
					case WorkDetailsFormState.CreateWork:
						Text = Labels.NewWork;
						FillDefaultFields();
						break;
					default:
						this.Text = Labels.TaskInformation;
						btnCancel.Text = Labels.Cancel;
						SwitchPanels(false);
						break;
				}
			}
		}

		public Func<WorkManagementService.ModifyParams, WorkManagementService.CloseResult> CloseWorkFunc { get; set; }

		public Func<WorkManagementService.ModifyParams, WorkManagementService.ReasonResult> AddReasonFunc { get; set; }

		public WorkDetailsForm()
		{
			InitializeComponent();
			this.SetFormStartPositionCenterScreen();

			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			Text = Labels.CloseWork;
			btnCancel.Text = Labels.Cancel;
			miCannedReasons.Text = Labels.Reasons;
			lblReason.Text = Labels.Reason + @":";
			btnCloseWork.Text = Labels.CloseWork;
			btnAddReason.Text = Labels.AddReason;
			lblTaskReasonsTitle.Text = Labels.TaskReasons + @":";
			colDate.HeaderText = Labels.Reason_Created;
			colReason.HeaderText = Labels.Reason_SelectedReason;
			colReasonText.HeaderText = Labels.Reason_TypedReason;
			lblName.Text = Labels.WorkData_Name;
			lblCategory.Text = Labels.WorkData_Category;
			lblDesc.Text = Labels.WorkData_Description;
			lblProject.Text = Labels.WorkData_Project;
			lblStart.Text = Labels.WorkData_Interval;
			lblPrio.Text = Labels.WorkData_Priority;
			lblDuration.Text = Labels.WorkData_TargetHours;
			lblProjWm.Text = Labels.SearchForWork;
			lblTotalWorkTime.Text = "";
			cbStart.Text = Labels.NewWork_StartWork;
			cbStart.Font = StyleUtils.GetFont(View.Controls.FontStyle.Light, 9.5f);
			toolTip1.SetToolTip(lblTotalWorkTime, Labels.WorkData_WorkedHours);
			toolTip1.SetToolTip(lblProject, Labels.WorkData_ProjectTooltip);
			toolTip1.SetToolTip(cbProject, Labels.WorkData_ProjectTooltip);
			dtpDuration.SetCueBanner(Labels.HourMinuteFormat, true);
			var p1 = Labels.WorkData_ProjectInstructions.IndexOf("[", StringComparison.InvariantCulture);
			var p2 = Labels.WorkData_ProjectInstructions.IndexOf("]", p1, StringComparison.InvariantCulture);
			llbProjectInstr.Text = Labels.WorkData_ProjectInstructions.Substring(0, p1) + Labels.WorkData_ProjectInstructions.Substring(p1 + 1, p2 - p1 - 1) + Labels.WorkData_ProjectInstructions.Substring(p2 + 1);
			llbProjectInstr.LinkArea = new LinkArea(p1, p2 - p1 - 1);

			if (pnlProject.Width < llbProjectInstr.Width) // extend the width of form with the size of localized instruction
			{
				pnlWorkInfo.Width += llbProjectInstr.Width - pnlProject.Width;
			}

			txtPriority.Invariant = x =>
			{
				if (string.IsNullOrEmpty(x)) return true;
				int i;
				return int.TryParse(x, out i) && i > 0;
			};

			Image result = new Bitmap(menuStrip1.ImageScalingSize.Width, menuStrip1.ImageScalingSize.Height);
			using (var g = Graphics.FromImage(result))
			{
				var q = result.Height / 8;
				g.FillPolygon(SystemBrushes.ControlText, new[] {
						new Point(result.Width / 2, result.Height * 3 / 4 - q),
						new Point(result.Width / 4, result.Height / 2 - q),
						new Point(result.Width * 3 / 4, result.Height / 2 - q)
					});
			}
			miCannedReasons.TextImageRelation = TextImageRelation.TextBeforeImage;
			miCannedReasons.Image = result;
			lblReasonLocation = lblReason.Location;
			txtReasonLocation = txtReason.Location;
			txtReasonHeight = txtReason.Height;

			HasPermission = false;

			State = WorkDetailsFormState.Information;

			Debug.Assert(context is WindowsFormsSynchronizationContext);
			cbCategory.DisplayMember = "Name";
			var categories = new List<CategoryHelperClass>();
			foreach (var x in MenuQuery.Instance.ClientMenuLookup.Value.AllCategoriesById.Values)
			{
				categories.Add(new CategoryHelperClass
				{
					Name = x.Name,
					Id = (int?)x.Id
				});
			}
			cbCategory.ValueMember = "Id";
			cbCategory.DataSource = categories;
			cbCategory.SetComboScrollWidth();

			MenuQuery.Instance.ClientMenuLookup.Changed += HandleClientMenuLookupChanged; //Unsubscribing at dispose event
			HandleClientMenuLookupChanged(this, EventArgs.Empty);
			log.Debug("WorkDetails window opened");
		}

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class CategoryHelperClass
		{
			public string Name { get; set; }
			public int? Id { get; set; }
		}

		public void RefreshTotalWorkTime(TimeSpan? workTime)
		{
			lblTotalWorkTime.Text = workTime != null ? workTime.Value.ToHourMinuteString() : "";
		}

		private void RefreshReasonList()
		{
			dgvReasonList.SuspendLayout();
			dgvReasonList.Rows.Clear();
			bool isReasonIdColumnFilled = false;
			if (reasons != null)
			{
				foreach (var item in reasons.OrderByDescending(r => r.createdAt))
				{
					var text = "";
					if (reasonTypeLookup != null && item.ReasonItemId.HasValue) isReasonIdColumnFilled |= reasonTypeLookup.TryGetValue(item.ReasonItemId.Value, out text);
					var createdAt = item.createdAt.ToLocalTime();
					var rowId = dgvReasonList.Rows.Add(createdAt.ToShortDateString() + " " + createdAt.ToShortTimeString(), text, item.ReasonText);
					dgvReasonList.Rows[rowId].Cells[1].ToolTipText = text;
					dgvReasonList.Rows[rowId].Cells[2].ToolTipText = item.ReasonText;
				}
			}

			dgvReasonList.Columns[1].Visible = isReasonIdColumnFilled;
			dgvReasonList.ResumeLayout();
			pReason.Visible = dgvReasonList.RowCount != 0;
		}

		public void RefreshReasons(CannedCloseReasons reasons)
		{
			miCannedReasons.DropDown.Items.ClearWithDispose();
			txtReason.ReadOnly = reasons != null && reasons.IsReadonly;
			if (txtReason.ReadOnly)
			{
				txtReason.GotFocus += (_, __) => WinApi.HideCaret(txtReason.Handle); //hide caret for readonly textbox
			}
			if (reasons == null || reasons.TreeRoot == null || reasons.TreeRoot.Count == 0)
			{
				int offsetY = menuStrip1.Location.Y + menuStrip1.Height / 2 - lblReason.Location.Y; //move work label up a bit if there is no menu
				lblReason.Top += offsetY;
				txtReason.Top += offsetY;
				txtReason.Height -= offsetY;
				menuStrip1.Visible = false;
				return;
			}
			menuStrip1.Visible = true;
			reasonTypeLookup = new Dictionary<int, string>();
			lblReason.Location = lblReasonLocation;
			txtReason.Location = txtReasonLocation;
			txtReason.Height = txtReasonHeight;

			miCannedReasons.DropDown.SuspendLayout(); //performance reasons
			foreach (var rootNode in reasons.TreeRoot)
			{
				AddReasonNode(rootNode, miCannedReasons, null);
			}

			RefreshReasonList();
			miCannedReasons.DropDown.ResumeLayout();
			miCannedReasons.ToolTipText = Labels.ChooseReasonHint;
		}

		private void RefreshProjectName()
		{
			Debug.Assert(MenuQuery.Instance.ClientMenuLookup.Value != null);
			if (workToModify != null)
			{
				if (workToModify.Id != null && MenuQuery.Instance.ClientMenuLookup.Value.ProjectByWorkId.ContainsKey(workToModify.Id.Value))
				{
					cbProject.SetSelectedItem(MenuQuery.Instance.ClientMenuLookup.Value.ProjectByWorkId[workToModify.Id.Value]);
				}
			}
		}

		private void RefreshOkButton()
		{
			switch (State)
			{
				case WorkDetailsFormState.AddReason:
					btnOk.Text = (isModified && HasPermission) ? string.Format("{0} && {1}", Labels.Preference_Save, Labels.AddReasonButton) : Labels.AddReasonButton;
					btnOk.Enabled = !isBusy;
					break;
				case WorkDetailsFormState.CloseWork:
					btnOk.Text = (isModified && HasPermission) ? string.Format("{0} && {1}", Labels.Preference_Save, Labels.CloseWorkButton) : Labels.CloseWorkButton;
					btnOk.Enabled = !isBusy;
					break;
				case WorkDetailsFormState.CreateWork:
					btnOk.Text = Labels.NewWork_Create;
					btnOk.Enabled = !isBusy && HasPermission;
					break;
				case WorkDetailsFormState.Information:
					btnOk.Text = Labels.Preference_Save;
					btnOk.Enabled = isModified && !isBusy && HasPermission;
					break;
				default:
					btnOk.Enabled = !isBusy;
					btnOk.Text = Labels.Ok;
					break;
			}

			btnOk.Width = btnOk.PreferredSize.Width;
		}

		private void FillDefaultFields()
		{
			var fields = NewWorkHelper.GetRepresentative();
			var clientMenu = MenuQuery.Instance.ClientMenuLookup.Value;
			if (fields.ProjectId != null && clientMenu.ProjectDataById.ContainsKey(fields.ProjectId.Value))
			{
				cbProject.SetSelectedWorkId(fields.ProjectId.Value);
			}

			if (fields.CategoryId != null) cbCategory.SelectedValue = fields.CategoryId;
			if (fields.StartOffset != null)
			{
				dtpStartDate.Checked = true;
				dtpStartDate.Value = DateTime.Now.Date.AddDays(fields.StartOffset.Value);
			}

			if (fields.StartOffset != null && fields.Length != null)
			{
				dtpEndDate.Checked = true;
				dtpEndDate.Value = dtpStartDate.Value.AddDays(fields.Length.Value);
			}

			if (fields.Description != null)
			{
				txtDescription.Text = fields.Description;
			}

			if (fields.Priority != null)
			{
				txtPriority.Text = fields.Priority.Value.ToString(CultureInfo.InvariantCulture);
			}

			if (fields.TargetWorkTime != null)
			{
				dtpDuration.Value = new TimeSpan(0, fields.TargetWorkTime.Value, 0);
			}

			cbStart.Checked = fields.StartNew;
		}

		private void AddReasonNode(CloseReasonNode node, ToolStripMenuItem parentMenu, string nodeText)
		{
			ToolStripMenuItem menuItem;
			if (string.IsNullOrEmpty(nodeText))
				nodeText = node.ReasonPart;
			else
				nodeText += " » " + node.ReasonPart;
			if (node.Children != null)
			{
				menuItem = new ToolStripMenuItem(node.ReasonPart, null);
				foreach (var item in node.Children)
					AddReasonNode(item, menuItem, nodeText);
			}
			else
			{
				menuItem = new ToolStripMenuItem(node.ReasonPart, null, HandleMenuItemClicked);
			}
			menuItem.Tag = new KeyValuePair<int, string>(node.NodeId, nodeText);
			parentMenu.DropDownItems.Add(menuItem);
			reasonTypeLookup.Add(node.NodeId, nodeText);
		}

		protected override void SetBusyImpl(bool isBusy)
		{
			btnCancel.Enabled = !isBusy;
			if (isBusy)
			{
				btnOk.Enabled = false;
			}
			else
			{
				RefreshOkButton();
			}

			cbStart.Enabled = !isBusy;
			txtReason.Enabled = !isBusy;
			menuStrip1.Enabled = !isBusy;
		}

		private void CreateWork(int projectId, WorkData work, bool switchToWork)
		{
			if (work == null) return;
			log.DebugFormat("Creating work \"{1}\" inside project #{0}", projectId, work.Name);
			SetBusy(true);
			var splashForm = new SplashForm(Labels.NewWork_State, Labels.NewWork_Creating);
			splashForm.Top = Top + Height / 2 - splashForm.Height / 2;
			splashForm.Left = Left + Width / 2 - splashForm.Width / 2;
			splashForm.Show(this);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var res = Service.CreateWork(projectId, work);
				context.Post(__ =>
				{
					if (IsDisposed) return;
					SetBusy(false);
					if (splashForm != null)
					{
						splashForm.Close();
						splashForm.Dispose();
						splashForm = null;
					}

					if (res.Exception != null)
					{
						MessageBox.Show(this, Labels.NewWork_CantCreate);
						if (res.Exception is System.ServiceModel.FaultException) //assume invalid cache
						{
							Service.RemoveProjectManagementConstraintsCache(projectId);
							UpdateProjectConstraints();
						}
					}
					else
					{
						StartWork = switchToWork ? new WorkData() { Id = res.Result } : null;
						DialogResult = DialogResult.OK;
						Close();
					}
				}, null);
			});
		}

		private void UpdateWork(int projectId, WorkData original, WorkData updated, Action continuation = null)
		{
			log.Debug("Updating work");
			SetBusy(true);
			var splashForm = new SplashForm(Labels.NewWork_State, Labels.NewWork_Updating);
			splashForm.Top = Top + Height / 2 - splashForm.Height / 2;
			splashForm.Left = Left + Width / 2 - splashForm.Width / 2;
			splashForm.Show(this);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				Debug.Assert(Service != null);
				var res = Service.UpdateWork(projectId, original, updated);
				context.Post(__ =>
				{
					if (IsDisposed) return;
					SetBusy(false);
					if (splashForm != null)
					{
						splashForm.Close();
						splashForm.Dispose();
						splashForm = null;
					}

					if (res.Exception != null)
					{
						MessageBox.Show(this, Labels.NewWork_CantUpdate);
						if (res.Exception is System.ServiceModel.FaultException) //assume invalid cache
						{
							Service.RemoveProjectManagementConstraintsCache(projectId);
							UpdateProjectConstraints();
						}
					}
					else
					{
						DialogResult = DialogResult.OK;
						if (continuation == null)
						{
							Close();
						}
						else
						{
							continuation();
						}
					}
				}, null);
			});
		}

		private void DetectModification()
		{
			var wd = GetWorkDataFromUI();
			if (wd == null)
			{
				isModified = false;
				return;
			}

			txtDescription.MaxLength = 1000 + wd.Description.Count(c => c == '\n');
			lblDesc.Text = string.Format("{0} ({1}/1000)", Labels.WorkData_Description, wd.Description.Length);
			if (workToModify != null)
			{
				var val = IsWorkDataModified(workToModify, wd);
				if (isModified != val)
				{
					isModified = val;
					HandleModifiedChanged();
				}
			}
		}

		private void UpdateProjectConstraints()
		{
			SetBusy(true);
			var wd = cbProject.SelectedItem as WorkDataWithParentNames;
			if (wd == null)
			{
				return;
			}

			ThreadPool.QueueUserWorkItem(_ =>
			{
				Debug.Assert(wd.WorkData.ProjectId != null);
				var constraints = Service.GetProjectManagementConstraintsOrCached(wd.WorkData.ProjectId.Value);
				context.Post(__ =>
				{
					if (IsDisposed) return;
					SetBusy(false);
					errorProvider.Clear();
					if (constraints == null)
					{
						errorProvider.SetIconAlignment(btnOk, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetIconPadding(btnOk, 5);
						errorProvider.SetError(btnOk, Labels.NewWork_NoConnection);
						HasPermission = false;
						return;
					}

					if (workToModify != null && ((ProjectManagementPermissions)constraints.ProjectManagementPermissions & ProjectManagementPermissions.ModifyWork) != ProjectManagementPermissions.ModifyWork)
					{
						errorProvider.SetIconAlignment(cbProject, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(cbProject, Labels.NewWork_NoPermUpdate);
						HasPermission = false;
						return;
					}

					if (workToModify == null &&
						((ProjectManagementPermissions)constraints.ProjectManagementPermissions &
						 ProjectManagementPermissions.CreateWork) != ProjectManagementPermissions.CreateWork)
					{
						errorProvider.SetIconAlignment(cbProject, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(cbProject, Labels.NewWork_NoPermCreate);
						HasPermission = false;
						return;
					}

					HasPermission = true;
					tableLayoutPanel1.SuspendLayout();
					Debug.Assert(constraints != null);
					var field = (ManagementFields)constraints.WorkMandatoryFields;
					lblPrio.FontWeight = ((field & ManagementFields.Priority) != 0) ? MetroLabelWeight.Bold : MetroLabelWeight.Light;
					lblPrio.Width = lblPrio.PreferredWidth;
					lblDesc.FontWeight = ((field & ManagementFields.Description) != 0) ? MetroLabelWeight.Bold : MetroLabelWeight.Light;
					lblDesc.Width = lblDesc.PreferredWidth;
					lblDuration.FontWeight = ((field & ManagementFields.TargetWorkTime) != 0) ? MetroLabelWeight.Bold : MetroLabelWeight.Light;
					lblDuration.Width = lblDuration.PreferredWidth;
					lblStart.FontWeight = ((field & ManagementFields.StartEndDate) != 0) ? MetroLabelWeight.Bold : MetroLabelWeight.Light;
					lblStart.Width = lblStart.PreferredWidth;
					lblCategory.FontWeight = ((field & ManagementFields.Category) != 0) ? MetroLabelWeight.Bold : MetroLabelWeight.Light;
					lblCategory.Width = lblStart.PreferredWidth;
					tableLayoutPanel1.ResumeLayout(true);
				}, null);
			});
		}

		private static bool IsWorkDataModified(WorkData originalWorkData, WorkData newWorkData)
		{
			Debug.Assert(originalWorkData != null && newWorkData != null);
			//Debug.Assert(originalWorkData.Id == newWorkData.Id);
			return originalWorkData.Name != newWorkData.Name
				   || originalWorkData.StartDate != newWorkData.StartDate
				   || originalWorkData.EndDate != newWorkData.EndDate
				   || originalWorkData.Description != newWorkData.Description
				   || originalWorkData.Priority != newWorkData.Priority
				   || originalWorkData.TargetTotalWorkTime != newWorkData.TargetTotalWorkTime
				   || originalWorkData.CategoryId != newWorkData.CategoryId;
		}

		private void CloseWork()
		{
			log.Debug("Closing work");
			SetBusy(true);
			var splashForm = new SplashForm(Labels.CloseWork, Labels.SplashMessage);
			splashForm.Top = Top + Height / 2 - splashForm.Height / 2;
			splashForm.Left = Left + Width / 2 - splashForm.Width / 2;
			splashForm.Show(this);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var result = CloseWorkFunc(new WorkManagementService.ModifyParams() { Reason = Reason, WorkData = WorkToModify, SelectedReasonId = selectedReasonId }); //this won't throw
				context.Post(__ =>
				{
					if (IsDisposed) return;
					SetBusy(false);
					if (splashForm != null)
					{
						splashForm.Close();
						splashForm.Dispose();
						splashForm = null;
					}
					DialogResult = DialogResult.None;
					if (result.Exception != null || result.Result == CloseWorkResult.UnknownError)
					{
						MessageBox.Show(this, Labels.CloseWorkErrorBody, Labels.CloseWorkErrorTitle);
					}
					else if (result.Result == CloseWorkResult.AlreadyClosed)
					{
						MessageBox.Show(this, Labels.CloseWorkErrorAlreadyClosedBody, Labels.CloseWorkErrorTitle);
					}
					else if (result.Result == CloseWorkResult.Ok)
					{
						DialogResult = DialogResult.OK;
					}
					else if (result.Result == CloseWorkResult.ReasonRequired)
					{
						MessageBox.Show(this, Labels.CloseWorkErrorReasonRequiredBody, Labels.CloseWorkErrorTitle);
						return; //don't close
					}
					else
					{
						Debug.Fail("invalid state");
						MessageBox.Show(this, Labels.CloseWorkErrorBody, Labels.CloseWorkErrorTitle);
					}
					this.Close();
				}, null);
			});
		}

		private void AddReason()
		{
			log.Debug("Adding reason");
			SetBusy(true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var result = AddReasonFunc(new WorkManagementService.ModifyParams() { Reason = Reason, WorkData = WorkToModify, SelectedReasonId = selectedReasonId }); //this won't throw
				context.Post(__ =>
				{
					if (IsDisposed) return;
					SetBusy(false);
					DialogResult = DialogResult.None;
					if (result.Exception != null)
					{
						MessageBox.Show(this, Labels.AddReasonErrorBody, Labels.AddReasonErrorTitle);
					}
					else if (result.ReasonRequired)
					{
						MessageBox.Show(this, Labels.AddReasonErrorReasonRequired, Labels.AddReasonErrorTitle);
						return; //don't close
					}
					else if (result.ReasonCount.HasValue)
					{
						DialogResult = DialogResult.OK;
					}
					else if (result.IsDeferred)
					{
						MessageBox.Show(this, Labels.AddReasonDelayedBody, Labels.AddReasonDelayedTitle);
						DialogResult = DialogResult.OK;
					}
					else
					{
						Debug.Fail("invalid state");
						MessageBox.Show(this, Labels.AddReasonErrorBody, Labels.AddReasonErrorTitle);
					}
					this.Close();
				}, null);
			});
		}

		//'manual binding'
		private WorkData GetWorkDataFromUI()
		{
			int priority;
			return new WorkData()
			{
				Id = workToModify != null ? workToModify.Id : null,
				Name = txtName.Text,
				Priority = string.IsNullOrEmpty(txtPriority.Text) || !int.TryParse(txtPriority.Text, out priority) ? (int?)null : priority,
				StartDate = dtpStartDate.Checked ? (DateTime?)dtpStartDate.Value : null,
				EndDate = dtpEndDate.Checked ? (DateTime?)dtpEndDate.Value : null,
				TargetTotalWorkTime = dtpDuration.Value,
				Description = CrLfRegex.Replace(txtDescription.Text, "\n"),
				CategoryId = cbCategory.SelectedIndex != -1 ? (int?)cbCategory.SelectedValue : null,
				IsForMobile = true,
			};
		}


		private void SwitchPanels(bool showEditReason)
		{
			if (!pnlEditReason.Visible && showEditReason)
			{
				pnlEditReason.Visible = true;
				btnAddReason.Visible = false;
				btnCloseWork.Visible = false;
				timer1.Start();
			}
			if (pnlEditReason.Visible && !showEditReason)
			{
				pnlEditReason.Visible = false;
				btnAddReason.Visible = true;
				btnCloseWork.Visible = workToModify != null && !workToModify.IsReadOnly;
			}
		}

		private bool ValidateConstraints(int projectId, WorkData origWorkData, WorkData newWorkData)
		{
			Debug.Assert(newWorkData != null);
			var constraints = Service.GetProjectManagementConstraintsCachedOnly(projectId);
			if (constraints == null)
			{
				log.ErrorAndFail("Constraints are not cached");
				ThreadPool.QueueUserWorkItem(_ => Service.GetProjectManagementConstraintsOrCached(projectId));
				//MessageBox.Show(Labels.NewWork_NoConnection, Labels.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}

			return (ValidateMandatory(constraints, newWorkData) && ValidateLimits(constraints, origWorkData, newWorkData));
		}

		private bool ValidateMandatory(ProjectManagementConstraints constraints, WorkData workData)
		{
			Debug.Assert(constraints != null);
			string property;
			if (
				!WorkManagementService.ValidateMandatoryFields((ManagementFields)constraints.WorkMandatoryFields, workData,
					out property))
			{
				switch (property)
				{
					case "CategoryId":
						errorProvider.SetIconAlignment(cbCategory, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(cbCategory, Labels.NewWork_Mandatory);
						return false;
					case "Description":
						errorProvider.SetIconAlignment(txtDescription, ErrorIconAlignment.TopLeft);
						errorProvider.SetError(txtDescription, Labels.NewWork_Mandatory);
						return false;
					case "Priority":
						errorProvider.SetIconAlignment(txtPriority, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(txtPriority, Labels.NewWork_Mandatory);
						return false;
					case "StartDate":
						errorProvider.SetIconAlignment(dtpStartDate, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(dtpStartDate, Labels.NewWork_Mandatory);
						return false;
					case "EndDate":
						errorProvider.SetIconAlignment(dtpEndDate, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(dtpEndDate, Labels.NewWork_Mandatory);
						return false;
					case "TargetTotalWorkTime":
						errorProvider.SetIconAlignment(dtpDuration, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(dtpDuration, Labels.NewWork_Mandatory);
						return false;
					default:
						errorProvider.SetIconAlignment(btnOk, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(btnOk, Labels.NewWork_UnknownInterval);
						return false;
				}
			}

			return true;
		}

		private bool ValidateLimits(ProjectManagementConstraints constraints, WorkData origWorkData, WorkData newWorkData)
		{
			Debug.Assert(newWorkData != null);
			string property;
			if (
				!WorkManagementService.ValidateLimits(constraints, newWorkData.StartDate, newWorkData.EndDate,
					WorkManagementService.Diff(origWorkData == null ? null : origWorkData.TargetTotalWorkTime, newWorkData.TargetTotalWorkTime), out property))
			{
				switch (property)
				{
					case "StartDate":
						var st = constraints.WorkMinStartDate.HasValue && constraints.WorkMinStartDate > newWorkData.StartDate ? string.Format(" ({0}: {1})", Labels.Min, constraints.WorkMinStartDate.Value.ToShortDateString()) : "";
						errorProvider.SetIconAlignment(dtpStartDate, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(dtpStartDate, Labels.NewWork_Interval + st);
						return false;
					case "EndDate":
						var en = constraints.WorkMaxEndDate.HasValue && constraints.WorkMaxEndDate.Value < newWorkData.EndDate ? string.Format(" ({0}: {1})", Labels.Max, constraints.WorkMaxEndDate.Value.ToShortDateString()) : "";
						errorProvider.SetIconAlignment(dtpEndDate, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(dtpEndDate, Labels.NewWork_Interval + en);
						return false;
					case "TargetTotalWorkTime":
						var origTarget = origWorkData == null ? new TimeSpan?() : origWorkData.TargetTotalWorkTime;
						var max = origTarget.GetValueOrDefault() + constraints.WorkMaxTargetWorkTime;
						var maxStr = max.HasValue ? string.Format(" ({0}: {1})", Labels.Max, max.ToHourMinuteString()) : "";
						errorProvider.SetIconAlignment(dtpDuration, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(dtpDuration, Labels.NewWork_Interval + maxStr);
						return false;
					default:
						errorProvider.SetIconAlignment(btnOk, ErrorIconAlignment.MiddleLeft);
						errorProvider.SetError(btnOk, Labels.NewWork_UnknownInterval);
						return false;
				}
			}

			return true;
		}

		private void HandleMenuItemClicked(object sender, EventArgs e)
		{
			Debug.Assert(sender is ToolStripMenuItem);
			TelemetryHelper.RecordFeature("ReasonTree", "Add");
			var item = (KeyValuePair<int, string>)(sender as ToolStripMenuItem).Tag;
			selectedReasonId = item.Key;
			lblReasonSelected.Text = item.Value;
			toolTip1.SetToolTip(lblReasonSelected, lblReasonSelected.Text);
			btnOk.Focus();
		}

		private void HandleCancelClicked(object sender, EventArgs e)
		{
			if (State == WorkDetailsFormState.AddReason || State == WorkDetailsFormState.CloseWork)
			{
				log.Debug("UI - Work details back clicked");
				State = WorkDetailsFormState.Information;
				return;
			}

			log.Debug("UI - Work details Cancel clicked");
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void HandleOkClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Work details Ok clicked");
			errorProvider.Clear();

			var proj = cbProject.SelectedItem as WorkDataWithParentNames;
			if (proj == null)
			{
				errorProvider.SetIconAlignment(cbProject, ErrorIconAlignment.MiddleLeft);
				errorProvider.SetError(cbProject, Labels.NewWork_SelectProject);
				return;
			}

			if (string.IsNullOrEmpty(txtName.Text))
			{
				errorProvider.SetIconAlignment(txtName, ErrorIconAlignment.MiddleLeft);
				errorProvider.SetError(txtName, Labels.NewWork_Mandatory);
				return;
			}

			if (!dtpDuration.ValidInput)
			{
				errorProvider.SetIconAlignment(dtpDuration, ErrorIconAlignment.MiddleLeft);
				errorProvider.SetError(dtpDuration, Labels.InvalidFormat);
				return;
			}

			Debug.Assert(proj.WorkData != null);
			Debug.Assert(proj.WorkData.ProjectId != null);
			var projId = proj.WorkData.ProjectId.Value;
			var wd = GetWorkDataFromUI();
			if (wd == null) return;
			if (workToModify != null)
			{
				log.DebugFormat("Modifying work {0} inside project {1}", workToModify.Id, projId);
				Action closeOrAdd = null;
				switch (State)
				{
					case WorkDetailsFormState.CloseWork:
						TelemetryHelper.RecordFeature("WorkDetails", "CloseWork");
						closeOrAdd = CloseWork;
						break;
					case WorkDetailsFormState.AddReason:
						TelemetryHelper.RecordFeature("WorkDetails", "AddReason");
						closeOrAdd = AddReason;
						break;
				}

				if (HasPermission && IsWorkDataModified(workToModify, wd) && ValidateConstraints(projId, workToModify, wd))
				{
					TelemetryHelper.RecordFeature("WorkDetails", "ModifyWork");
					UpdateWork(projId, workToModify, wd, closeOrAdd);
				}
				else if (closeOrAdd != null)
				{
					closeOrAdd();
				}
			}
			else
			{
				log.DebugFormat("Creating work inside project {0}", projId);
				if (!HasPermission || !ValidateConstraints(projId, null, wd)) return;
				CreateWork(projId, wd, cbStart.Checked);
				TelemetryHelper.RecordFeature("WorkDetails", "CreateWork");
				NewWorkHelper.AddRecent(wd, projId, cbStart.Checked);
			}
		}

		private void HandleModifiedChanged()
		{
			RefreshOkButton();
		}


		private void HandleAddReasonClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Work details Add reason clicked");
			State = WorkDetailsFormState.AddReason;
		}

		private void HandleCloseWorkClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Work details Close clicked");
			State = WorkDetailsFormState.CloseWork;
		}

		private void HandleProjectChanged(object sender, EventArgs e)
		{
			if (workToModify == null || !workToModify.IsReadOnly) UpdateProjectConstraints();
		}

		private void HandleInputChanged(object sender, EventArgs e)
		{
			if (workToModify == null || !workToModify.IsReadOnly) DetectModification();
		}

		private void HandleTimerTicked(object sender, EventArgs e)
		{
			timer1.Enabled = false;
			toolTip1.Show(toolTip1.GetToolTip(menuStrip1), menuStrip1, 5000);
		}

		protected void HandleClientMenuLookupChanged(object sender, EventArgs e)
		{
			RefreshProjectName();
			cbProject.UpdateMenu(MenuQuery.Instance.ClientMenuLookup.Value);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleClientMenuLookupChanged;

			base.Dispose(disposing);
		}

		private void lblProjWm_Click(object sender, EventArgs e)
		{
			cbProject.Select();
		}

		private void cbProject_TextChanged(object sender, EventArgs e)
		{
			lblProjWm.Visible = string.IsNullOrEmpty(cbProject.Text);
		}

		private void llbProjectInstr_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RecentUrlQuery.Instance.OpenLink(ConfigManager.WebsiteUrlFormatString + "Tasks/Default.aspx");
		}
	}
}
