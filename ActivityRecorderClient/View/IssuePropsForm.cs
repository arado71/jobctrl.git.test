using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;

namespace Tct.ActivityRecorderClient.View
{
	public partial class IssuePropsForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private IssuePropColumnFlag issuePropColumns;
		private bool forceHide;
		private int panelHeight;
		private bool isInitializing;

		private const uint WS_POPUP = 0x80000000;
		private const int WS_EX_TOPMOST = 0x00000008;
		private const int WM_MOUSEMOVE = 0x0200;
		private const int WM_MOUSELEAVE = 0x02A3;
		private const int WM_NCMOUSELEAVE = 0x02A2;
		private const int WM_NCMOUSEMOVE = 0x00A0;

		private readonly Dictionary<IssueState, IssueStateItem> statesDict = new Dictionary<IssueState, IssueStateItem>();

		// http://stackoverflow.com/questions/986529/how-to-detect-if-the-mouse-is-inside-the-whole-form-and-child-controls
		// 
		private MouseMoveMessageFilter mouseMessageFilter;
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			panelHeight = tlpIssueEdit.Height;
			lblCompany.Visible = cbCompany.Visible = issuePropColumns.HasFlag(IssuePropColumnFlag.CompanyVisible);
			if (!lblCompany.Visible) panelHeight -= Math.Max(lblCompany.Height, cbCompany.Height);
			lblName.Visible = cbName.Visible = issuePropColumns.HasFlag(IssuePropColumnFlag.NameVisible);
			if (!lblName.Visible) panelHeight -= Math.Max(lblName.Height, cbName.Height);
			lblCategory.Visible = cbCategory.Visible = issuePropColumns.HasFlag(IssuePropColumnFlag.CategoryVisible);
			if (!lblCategory.Visible) panelHeight -= Math.Max(lblCategory.Height, cbCategory.Height);
			lblState.Visible = cbState.Visible = issuePropColumns.HasFlag(IssuePropColumnFlag.StateVisible);
			btnIssues.Visible = issuePropColumns.HasFlag(IssuePropColumnFlag.IssuesButtonVisible);
			if (!lblState.Visible && !btnIssues.Visible) panelHeight -= Math.Max(Math.Max(lblState.Height, cbState.Height), btnIssues.Height);
			tlpIssueEdit.Height = 0;

			mouseMessageFilter = new MouseMoveMessageFilter { TargetForm = this };
			Application.AddMessageFilter(mouseMessageFilter);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			Application.RemoveMessageFilter(mouseMessageFilter);
		}
		public IssuePropsForm(IssuePropColumnFlag issuePropColumns)
		{
			this.issuePropColumns = issuePropColumns;
			InitializeComponent();
			Localize();
		}

		private void Localize()
		{
			lblName.Text = Labels.IssueMgr_Name + @":";
			lblCompany.Text = Labels.IssueMgr_Company + @":";
			lblCategory.Text = Labels.IssueMgr_Category + @":";
			lblState.Text = Labels.IssueMgr_State + @":";
			btnIssues.Text = Labels.IssueMgr_Issues;

			foreach (IssueState value in Enum.GetValues(typeof(IssueState)))
			{
				var stateItem = new IssueStateItem
				{
					Caption = Labels.ResourceManager.GetString("IssueMgr_State" + value),
					State = value,
				};
				cbState.Items.Add(stateItem);
				statesDict.Add(value, stateItem);
			}
		}

		private void IssuePropsFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (Visible)
			{
				e.Cancel = true;
				Visible = false;
				forceHide = true;
			}
		}

		public new void Show()
		{
			if (!forceHide)
				base.Show();
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var createParams = base.CreateParams;
				createParams.Style = (int) (createParams.Style | WS_POPUP);
				createParams.ExStyle |= WS_EX_TOPMOST;
				return createParams;				
			}
		}

		protected override bool ShowWithoutActivation { get { return true; } }

		public string IssueName { get { return cbName.Text; } set { cbName.Text = value; } }
		public string Company { get { return cbCompany.Text; } set { cbCompany.Text = value; } }
		public string Category { get { return issuePropColumns.HasFlag(IssuePropColumnFlag.CategoryVisible) ? cbCategory.Text : null; } set { if (issuePropColumns.HasFlag(IssuePropColumnFlag.CategoryVisible)) cbCategory.Text = value; } }
		public IssueState? IssueState { get { return (cbState.SelectedItem != null ? (IssueState?)((IssueStateItem)cbState.SelectedItem).State : null); } set { cbState.SelectedItem = value.HasValue ? statesDict[value.Value] : null; IsModified = false; } }
		public bool IsModified { get; private set; }
		public bool IsBusy { get { return !Enabled; } set { Enabled = !value; }}

		public event EventHandler PulledUp;
		public event EventHandler DroppedDown;
		public event EventHandler IssuesButtonClicked;

		public bool IsActivated { get; private set; }
		private void ComboBoxTextChanged(object sender, EventArgs e)
		{
			if (isInitializing) return;
			SetTitle();
			IsModified = true;
		}

		class MouseMoveMessageFilter : IMessageFilter
		{
			public IssuePropsForm TargetForm { get; set; }

			public bool PreFilterMessage(ref Message m)
			{
				if (!TargetForm.CheckCursorIsInsideControl()) return false;
				var numMsg = m.Msg;
				switch (numMsg)
				{
					case WM_MOUSEMOVE:
					case WM_NCMOUSEMOVE:
						if (MousePosition.X > TargetForm.Left && MousePosition.X <= TargetForm.Left + TargetForm.Width &&
						    MousePosition.Y > TargetForm.Top && MousePosition.Y <= TargetForm.Top + TargetForm.Height)
						{
							if (TargetForm.tlpIssueEdit.Height == 0)
							{
								log.Debug("panelHideTimer.Start");
								TargetForm.panelHideTimer.Start();
								TargetForm.tlpIssueEdit.Height = TargetForm.panelHeight;
								var del = TargetForm.DroppedDown;
								if (del != null)
									del(TargetForm, EventArgs.Empty);
							}
						}
						break;
					case WM_NCMOUSELEAVE:
					case WM_MOUSELEAVE:
						// we can't use, because MouseLeave events triggered at the wrong time
						break;
				}
				

				return false;
			}

		}

		private void panelHideTimer_Tick(object sender, EventArgs e)
		{
			if (!panelHideTimer.Enabled) return;
			if (IsActivated || MousePosition.X >= Bounds.X && MousePosition.X <= Bounds.Right && MousePosition.Y >= Bounds.Y && MousePosition.Y <= Bounds.Bottom) return;
			panelHideTimer.Stop();
			log.Debug("PullUp");
			PullUp();
		}

		private void PullUp()
		{
			if (tlpIssueEdit.Height <= 0) return;
			tlpIssueEdit.Height = 0;
			var del = PulledUp;
			if (del != null)
				del(this, EventArgs.Empty);
		}

		internal void InitFields(string name, string company, string category, IssueState? issueState)
		{
			isInitializing = true;
			if (LocalIssueNamesAndCompanies.NameDict != null && LocalIssueNamesAndCompanies.NameDict.Count > 0)
			{
				cbName.DataSource = new BindingSource(LocalIssueNamesAndCompanies.NameDict, null);
				cbName.DisplayMember = cbName.ValueMember = "Key";
			}
			if (LocalIssueNamesAndCompanies.CompanyDict != null && LocalIssueNamesAndCompanies.CompanyDict.Count > 0)
			{
				cbCompany.DataSource = new BindingSource(LocalIssueNamesAndCompanies.CompanyDict, null);
				cbCompany.DisplayMember = cbCompany.ValueMember = "Key";
			}
			if (LocalIssueNamesAndCompanies.CategoryDict != null && LocalIssueNamesAndCompanies.CategoryDict.Count > 0)
			{
				cbCategory.DataSource = new BindingSource(LocalIssueNamesAndCompanies.CategoryDict, null);
				cbCategory.DisplayMember = cbCategory.ValueMember = "Key";
			}

			cbCategory.BindingContext = cbCompany.BindingContext = cbName.BindingContext = this.BindingContext;
			IssueName = name;
			Company = company;
			Category = category;
			IssueState = issueState;
			cbName.Select();
			cbName.SelectAll();
			SetTitle();
			isInitializing = false;
			IsModified = false;
			forceHide = false;
		}

		private const string FormTitleFormat = "*DEBUG* [{0}] {1} ({2})";
		private void SetTitle()
		{
			var buf = new StringBuilder();
#if DEBUG
			buf.Append("*DEBUG* ");
#endif
			if (issuePropColumns.HasFlag(IssuePropColumnFlag.CompanyVisible)) buf.AppendFormat("[{0}] ", Company);
			if (issuePropColumns.HasFlag(IssuePropColumnFlag.NameVisible)) buf.AppendFormat("{0} ", IssueName);
			if (issuePropColumns.HasFlag(IssuePropColumnFlag.StateVisible)) buf.AppendFormat("({0}) ", cbState.SelectedItem ?? "-");
			if (issuePropColumns.HasFlag(IssuePropColumnFlag.CategoryVisible)) buf.AppendFormat("*{0}* ", Category);
			Text = buf.ToString();
		}

		private void IssuePropsFormActivated(object sender, EventArgs e)
		{
			IsActivated = true;
			categoryManualEntry = companyManualEntry = nameManualEntry = false;
		}

		private void IssuePropsFormDeactivate(object sender, EventArgs e)
		{
			IsActivated = false;
			if (IsModified && (categoryManualEntry || LocalIssueNamesAndCompanies.CategoryDict.ContainsKey(cbCategory.Text)) && !cbCategory.Text.IsNullOrWhiteSpace()) LocalIssueNamesAndCompanies.CategoryDict[cbCategory.Text] = DateTime.Now;
			if (IsModified && (companyManualEntry || LocalIssueNamesAndCompanies.CompanyDict.ContainsKey(cbCompany.Text)) && !cbCompany.Text.IsNullOrWhiteSpace()) LocalIssueNamesAndCompanies.CompanyDict[cbCompany.Text] = DateTime.Now;
			if (IsModified && (nameManualEntry || LocalIssueNamesAndCompanies.NameDict.ContainsKey(cbName.Text)) && !cbName.Text.IsNullOrWhiteSpace()) LocalIssueNamesAndCompanies.NameDict[cbName.Text] = DateTime.Now;
			PullUp();
		}

		private void btnIssues_Click(object sender, EventArgs e)
		{
			var del = IssuesButtonClicked;
			if (del != null)
				del(this, EventArgs.Empty);
		}


		internal void Ping()
		{
			formInactivationTimer.Stop();
			formInactivationTimer.Interval = Math.Max(ConfigManager.RuleMatchingInterval * 5,
				PluginMail.CaptureCachingDurationInSeconds * 1000 + ConfigManager.RuleMatchingInterval * 2);
			formInactivationTimer.Start();
		}

		private void formInactivationTimer_Tick(object sender, EventArgs e)
		{
			if (IsActivated) return;
			formInactivationTimer.Stop();
			PullUp();
			Visible = false;
		}

		public Capturing.Plugins.Impl.IssueNamesAndCompanies LocalIssueNamesAndCompanies { get; set; }
		private bool companyManualEntry;
		private bool categoryManualEntry;
		private bool nameManualEntry;


		private void cbNameKeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsLetterOrDigit(e.KeyChar))
			{
				nameManualEntry = true;
			}
		}

		private void cbCompanyKeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsLetterOrDigit(e.KeyChar))
			{
				companyManualEntry = true;
			}
		}

		private void cbCategoryKeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsLetterOrDigit(e.KeyChar))
			{
				categoryManualEntry = true;
			}
		}

	}

	public enum IssueState
	{
		Opened, Closed, WaitingForCustomer, 
	}

	internal class IssueStateItem
	{
		public IssueState State { get; set; }
		public string Caption { get; set; }
		public override string ToString()
		{
			return Caption;
		}
	}
}
