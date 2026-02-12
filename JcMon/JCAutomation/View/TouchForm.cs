using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using JcExtract;
using JCAutomation.Data;
using JCAutomation.Managers;
using JCAutomation.Properties;
using JCAutomation.SystemAdapter;
using Tct.ActivityRecorderClient;

namespace JCAutomation.View
{
	public partial class TouchForm : Form
	{
		private readonly CaptureManager captureManager = new CaptureManager();
		private readonly RecordManager recordManager = new RecordManager();
		private SynchronizationContext guiContext;
		private ControlInfo lastControl;

		private int workCounter = 0;

		public TouchForm()
		{
			InitializeComponent();
			captureManager.Capture += HandleCapture;
		}

		public void AddWork()
		{
			if (Interlocked.Increment(ref workCounter) == 1)
			{
				guiContext.Post(_ => SetLoading(true), null);
			}
		}

		public void FinishWork()
		{
			if (Interlocked.Decrement(ref workCounter) == 0)
			{
				guiContext.Post(_ => SetLoading(false), null);
			}
		}

		private void SetLoading(bool isLoading)
		{
			progressBar1.Visible = isLoading;
		}

		private void HandleCapture(object sender, CaptureEventArgs eventArgs)
		{
			recordManager.AddCapture(eventArgs.ActiveControl);
			guiContext.Post(_ => UpdateControls(eventArgs.ActiveControl), null);
		}

		private void UpdateWindow(WindowInfo info)
		{
			lblWinTitle.Text = info != null ? info.Title : "-";
			lblWinClass.Text = info != null ? info.ClassName : "-";
			lblWinProcess.Text = info != null ? info.ProcessName : "-";
		}

		private void RegisterHotkey(ModifierKeys modifier, Keys key)
		{
			if (WinApi.RegisterHotKey(Handle, 0, (uint) modifier, key) != 0)
			{
				MessageBox.Show(this, "Unable to register hotkey", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void UpdateControls(ControlInfo info)
		{
			lastControl = info;
			lblCtrlName.Text = info != null ? info.Name : "-";
			lblCtrlClass.Text = info != null ? info.ClassName : "-";
			lblCtrlAutomationId.Text = info != null ? info.AutomationId : "-";
			lblCtrlControlType.Text = info != null ? info.ControlType : "-";
			lblCtrlHelpText.Text = info != null ? info.HelpText : "-";
			lblCtrlSelection.Text = info != null ? info.Selection : "-";
			lblCtrlText.Text = info != null ? info.Text : "-";
			lblCtrlValue.Text = info != null ? info.Value : "-";
			if(info != null) UpdateWindow(info.GetWindowInfo());
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == (int)WinApi.Messages.WM_HOTKEY)
			{
				var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
				var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);
				TakeNote();
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var baseParams = base.CreateParams;
				baseParams.ExStyle |= (int)WinApi.WS_EX.TOPMOST;
				return baseParams;
			}
		}

		private void TakeNote()
		{
			var factory = new ControlInfoFactory()
			{
				IncludeComInterface = Configuration.CaptureCom,
				IncludeScreenshots = Configuration.IncludeScreenshots,
			};

			AddWork();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var controlGraph = factory.Get(lastControl.Element, DetailLevel.WithSiblings, DetailLevel.WithSiblings);
				guiContext.Post(__ =>
				{
					FinishWork();
					var form = new AddNote(this, controlGraph);
					form.ShowDialog(this);
				}, null);
			}, null);
		}

		private void HandleActivated(object sender, EventArgs e)
		{
			Opacity = 1.0;
		}

		private void HandleDeactivated(object sender, EventArgs e)
		{
			if (Disposing) return;
			Opacity = 0.6;
		}

		private void HandleLoaded(object sender, EventArgs e)
		{
			this.Icon = new Icon(this.Icon, this.Icon.Size); 
			guiContext = SynchronizationContext.Current;
			captureManager.Start();
			recordManager.Start(Configuration.RecordInterval);
			RegisterHotkey(System.Windows.Input.ModifierKeys.Control, Keys.F11);
		}

		private void HandleSaveCaptureClicked(object sender, EventArgs e)
		{
			recordManager.Save();
		}

		private void HandleModeClicked(object sender, EventArgs e)
		{
			captureManager.FocusMode = !captureManager.FocusMode;
			if (captureManager.FocusMode)
			{
				modeMenuStrip.Text = "Focus mode";
			}
			else
			{
				modeMenuStrip.Text = "Hover mode";
			}
		}

		private void HandleSettingsClicked(object sender, EventArgs e)
		{
			var settingsForm = new Settings();
			settingsForm.ShowDialog(this);
		}

		private void HandleFormClosing(object sender, FormClosingEventArgs e)
		{
			captureManager.Stop();
			recordManager.Stop();
		}

		private void tableLayoutPanel1_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
		{
			e.Graphics.FillRectangle(e.Row % 2 == 0 ? Brushes.LightSteelBlue : Brushes.SteelBlue, e.CellBounds);
		}
	}
}
