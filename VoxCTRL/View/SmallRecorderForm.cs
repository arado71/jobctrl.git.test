using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VoxCTRL.Controller;
using VoxCTRL.Voice;

namespace VoxCTRL.View
{
	public partial class SmallRecorderForm : Form
	{
		private static readonly Color recordColor = Color.FromArgb(204, 0, 0);
		private static readonly Color pauseColor = Color.FromArgb(200, 100, 0);
		private static readonly Color stopColor = Color.FromArgb(207, 207, 207);

		private readonly RecorderFormController controller;
		private bool buttonsEnabled;

		public SmallRecorderForm()
			: this(null)
		{
		}

		public SmallRecorderForm(RecorderFormController recorderFormController)
		{
			Icon = Properties.Resources.VoxCTRL; //don't set it in the designer as it would enlarge the exe

			InitializeComponent();

			toolTip.SetToolTip(btnRecord, "Felvétel indítása");
			toolTip.SetToolTip(btnStop, "Felvétel mentése");
			toolTip.SetToolTip(btnPause, "Felvétel megállítása");
			toolTip.SetToolTip(txtId, "Azonosító");

			var dragger = new FormDragger(this);
			dragger.AddDraggable(this);
			dragger.AddDraggable(pnlMain);
			dragger.AddDraggable(lblTime);
			dragger.AddDraggable(txtId);
			dragger.AddDraggable(picTooSilent);
			(this.components ?? (this.components = new Container())).Add(new ComponentWrapper(dragger));

			controller = recorderFormController ?? new RecorderFormController();
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "State") SetState(controller.State); };
			//controller.PropertyChanged += (_, e) => { if (e.PropertyName == "IsRecordingTooSilent") lblTime.ForeColor = !controller.IsRecordingTooSilent ? Color.Black : Color.White; };
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "IsRecordingTooSilent") picTooSilent.Visible = controller.IsRecordingTooSilent; };

			SetState(RecordingState.Stopped);

			lblTime.DataBindings.Add("Text", controller, "RecordingTime");
			txtId.DataBindings.Add("Text", controller, "RecordingName", false, DataSourceUpdateMode.OnPropertyChanged);
			this.DataBindings.Add("Text", controller, "Title");
		}

		//don't show caption even though we have one and JobCTRL could capture it
		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.Style &= (~0x00C00000); // WS_CAPTION
				//cp.Style &= (~0x00800000); // WS_BORDER
				//cp.ExStyle = 0x00000080 | 0x00000008; // WS_EX_TOOLWINDOW | WS_EX_TOPMOST
				return cp;
			}
		}

		private void SetState(RecordingState state)
		{
			switch (state)
			{
				case RecordingState.StopRequested:
					break;
				case RecordingState.Stopped:
					this.BackColor = stopColor;
					RefreshControls();
					break;
				case RecordingState.Recording:
					this.BackColor = recordColor;
					break;
				case RecordingState.PauseRequested:
					break;
				case RecordingState.Paused:
					this.BackColor = pauseColor;
					break;
				default:
					throw new ArgumentOutOfRangeException("state");
			}

			//copy paste from RecorderForm :(
			btnStop.Enabled = (state == RecordingState.Recording || state == RecordingState.Paused) && buttonsEnabled;
			btnRecord.Visible = state == RecordingState.Stopped || state == RecordingState.Paused || state == RecordingState.StopRequested;
			btnRecord.Enabled = (state == RecordingState.Stopped || state == RecordingState.Paused) && buttonsEnabled;
			btnPause.Visible = state == RecordingState.Recording || state == RecordingState.PauseRequested;
			btnPause.Enabled = state == RecordingState.Recording && buttonsEnabled;
			//end of copy paste
		}

		private void btnRecord_Click(object sender, EventArgs e)
		{
			controller.RecordOrResume();
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			//todo don't copy/paste this check ;/
			if (Properties.Settings.Default.IsNameMandatory && string.IsNullOrEmpty(controller.RecordingName))
			{
				MessageBox.Show(this, "Az azonosító megadása kötelező", "Hiba!");
				return;
			}
			controller.Stop();
		}

		private void btnPause_Click(object sender, EventArgs e)
		{
			controller.Pause();
		}

		private void RefreshControls()
		{
			buttonsEnabled = ConfigManager.IsManualStartStopEnabled;
		}

		public void UpdateSmallForm(RecordingState state)
		{
			SetState(state);
		}
	}
}
