using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using log4net;
using VoxCTRL.ActivityRecorderServiceReference;
using VoxCTRL.Controller;
using VoxCTRL.InteropService;
using VoxCTRL.Voice;
using VoxCTRL.Voice.Codecs;
using Tct.ActivityRecorderClient.View;
using System.ServiceModel;
using System.Threading;
using VoxCTRL.Communication;
using VoxCTRL.VersionReporting;

namespace VoxCTRL.View
{
	//todo hax don't upload on delete
	//todo progressbar or at least number of items to upload.
	//todo VoiceRecordingGenerator tests
	//todo handle bad password
	public partial class RecorderForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Update.UpdateManager updater;
		private readonly RecorderFormController controller;
		private readonly SmallRecorderForm smallForm;
		private readonly ServiceHost serviceHost;
		private readonly SynchronizationContext guiContext;
		private ActiveDirectoryAuthenticationManager ADAuthManager;
		private readonly ClientSettingsManager clientSettingsManager = new ClientSettingsManager();
		private readonly VersionReportManager versionReportManager = new VersionReportManager();
		private RecordingState actualState;
		private bool buttonsEnabled;
		private bool isAgcOn;
		private AutoGainController agc;

		public RecorderForm()
		{
			if (Properties.Settings.Default.IsUpgradeRequired)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.IsUpgradeRequired = false;
				Properties.Settings.Default.Save();
			}
			controller = new RecorderFormController();
			serviceHost = new ServiceHost(new VoiceRecorderControllerService(controller));

			Icon = Properties.Resources.VoxCTRL; //don't set it in the designer as it would enlarge the exe

			InitializeComponent();

			if (this.components == null)
			{
				this.components = new Container();
			}
			this.components.Add(new ComponentWrapper(controller));

			controller.LoadRecordings();
			cbDevice.DataSource = controller.RecordingDevices;
			cbDevice.SetComboScrollWidth();
			gridRecordings.DataSource = controller.VoiceRecordings;
			gridRecordings.CellFormatting += CellFormatting;
			controller.DataAvailable += DataAvailable;
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "State") SetState(controller.State); };
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "RecordingTime" && controller.RecordingTime == TimeSpan.Zero) ClearGuiAndAgc(); };
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "IsRecordingTooSilent")  lblTime.ForeColor = !controller.IsRecordingTooSilent ? Color.FromArgb(255, 153, 51) : Color.White; };
			controller.PropertyChanged += (_, e) => { if (e.PropertyName == "IsRecordingTooSilent") picTooSilent.Visible = controller.IsRecordingTooSilent; };
			controller.TooSilentRecordingNotification += TooSilentRecordingNotification;

			SetState(controller.State);

			lblTime.DataBindings.Add("Text", controller, "RecordingTime");
			txtId.DataBindings.Add("Text", controller, "RecordingName", false, DataSourceUpdateMode.OnPropertyChanged);
			this.DataBindings.Add("Text", controller, "Title");

			smallForm = new SmallRecorderForm(controller);

			UpdateStatusWindowVisibility();

			guiContext = AsyncOperationManager.SynchronizationContext;
			updater = new Update.UpdateManager(guiContext, controller); //controller.PropertyChanged registration after the gui
			updater.Start();
		}

		void TooSilentRecordingNotification(object sender, EventArgs e)
		{
			guiContext.Post(_ =>
				MessageBox.Show(this, "Túl alacsony a beérkező jelszint!\nKérem ellenőrizze felvételi jelszintet vagy a mikrofont!",
					"Alacsony jelszint", MessageBoxButtons.OK), null);
		}

		private void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.Value == null) return;
			if (e.ColumnIndex == createDateDataGridViewTextBoxColumn.Index)
			{
				DateTime date;
				if (!DateTime.TryParse(e.Value.ToString(), out date)) return;
				e.Value = TimeZone.CurrentTimeZone.ToLocalTime(date).ToString();
				e.FormattingApplied = true;
			}
			else if (e.ColumnIndex == lengthDataGridViewTextBoxColumn.Index)
			{
				int ms;
				if (!int.TryParse(e.Value.ToString(), out ms)) return;
				e.Value = ms >= 0 ? TimeSpan.FromMilliseconds(ms).ToHourMinuteSecondString() : "n/a";
				e.FormattingApplied = true;
			}
		}

		private void DataAvailable(object sender, RecordingDeviceEventArgs e)
		{
			double volumeToAgc;
			if (waveformPainter1.Visible)
			{
				waveformPainter1.AddMax(e.MaxVolume);
				volumeMeter1.Amplitude = e.MaxVolume;
				volumeToAgc = e.MaxVolume;
			}
			else
			{
				waveformPainter2.AddMax(e.MinVolume);
				waveformPainter3.AddMax(e.MaxVolume);
				volumeMeter2.Amplitude = e.MinVolume;
				volumeMeter3.Amplitude = e.MaxVolume;
				volumeToAgc = (e.MinVolume + e.MaxVolume) / 2.0;
			}
			lblBytesWritten.Text = ((e.BytesWritten + 1023) / 1024).ToString();
			if (agc != null) agc.AddSample(volumeToAgc);
		}

		private void SetState(RecordingState state)
		{
			switch (state)
			{
				case RecordingState.StopRequested:
					break;
				case RecordingState.Stopped:
					tbVolume.DataBindings.Clear();
					btnRecord.Text = "Felvétel indítása";
					RefreshControls();
					break;
				case RecordingState.Recording:
					if (tbVolume.DataBindings.Count != 0) return;
					tbVolume.Value = (int)controller.CurrentDevice.Volume;
					tbVolume.DataBindings.Add("Value", controller.CurrentDevice, "Volume", false, DataSourceUpdateMode.OnPropertyChanged);
					break;
				case RecordingState.PauseRequested:
					break;
				case RecordingState.Paused:
					btnRecord.Text = "Felvétel folytatása";
					break;
				default:
					throw new ArgumentOutOfRangeException("state");
			}

			btnStop.Enabled = (state == RecordingState.Recording || state == RecordingState.Paused) && buttonsEnabled;
			btnRecord.Visible = state == RecordingState.Stopped || state == RecordingState.Paused || state == RecordingState.StopRequested;
			btnRecord.Enabled = (state == RecordingState.Stopped || state == RecordingState.Paused) && buttonsEnabled;
			btnPause.Visible = state == RecordingState.Recording || state == RecordingState.PauseRequested;
			btnPause.Enabled = state == RecordingState.Recording && buttonsEnabled;
			btnDelete.Enabled = (state == RecordingState.Recording || state == RecordingState.Paused) && buttonsEnabled;
			cbDevice.Enabled = state == RecordingState.Stopped;
			cbQuality.Enabled = state == RecordingState.Stopped && !ConfigManager.Quality.HasValue;
			tbVolume.Enabled = state != RecordingState.Stopped && state != RecordingState.StopRequested;

			actualState = state;
		}

		private void cbDevice_SelectedIndexChanged(object sender, EventArgs e)
		{
			controller.CurrentDevice = cbDevice.SelectedIndex == -1 ? null : controller.RecordingDevices[cbDevice.SelectedIndex];
			if (controller.CurrentDevice == null) return;
			SetState(controller.CurrentDevice.State);
			cbQuality.DataSource = controller.CurrentDevice.GetSupportedWaveFormats().ToList();
			cbQuality.SetComboScrollWidth();
		}

		private void cbQuality_SelectedIndexChanged(object sender, EventArgs e)
		{
			var formats = cbQuality.DataSource as List<WaveFormatData>;
			if (controller.CurrentDevice == null || cbQuality.SelectedIndex == -1 || formats == null) return;
			var curr = formats[cbQuality.SelectedIndex];
			if (curr.Channels == 1)
			{
				volumeMeter1.Visible = true;
				volumeMeter2.Visible = false;
				volumeMeter3.Visible = false;

				waveformPainter1.Visible = true;
				waveformPainter2.Visible = false;
				waveformPainter3.Visible = false;
			}
			else
			{
				volumeMeter1.Visible = false;
				volumeMeter2.Visible = true;
				volumeMeter3.Visible = true;

				waveformPainter1.Visible = false;
				waveformPainter2.Visible = true;
				waveformPainter3.Visible = true;
			}
			controller.CurrentDevice.WaveFormat = curr;
		}

		private void btnRecord_Click(object sender, EventArgs e)
		{
			controller.RecordOrResume();
		}

		private void ClearGuiAndAgc()
		{
			ClearGui();
			if (agc != null) agc.Clear();
		}

		private void ClearGui()
		{
			waveformPainter1.Clear();
			waveformPainter2.Clear();
			waveformPainter3.Clear();
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
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

		private void btnDelete_Click(object sender, EventArgs e)
		{
			var res = MessageBox.Show(this, "Biztosan eldobja a felvételt", "Törlés jóváhagyása", MessageBoxButtons.YesNo);
			if (res != DialogResult.Yes) return;
			controller.StopAndDelete();
		}

		private void RecorderForm_Load(object sender, EventArgs e)
		{
			log.Info("Form loading");
			AutoStartHelper.Register(updater);
			clientSettingsManager.LoadSettings();
			log.Debug("ClientSettingsManager Loaded");
			clientSettingsManager.Start();
			log.Debug("ClientSettingsManager Started");
			clientSettingsManager.SettingsChanged += ClientSettingsManagerOnSettingsChanged;
			versionReportManager.Start();
			log.Debug("VersionReportManager Started");
			if (ActiveDirectoryLoginServiceClientWrapper.IsActiveDirectoryAuthEnabled && ConfigManager.UserPasswordExpirationDate.HasValue) //ugly hax
			{
				log.Info("ActiveDirectoryAuthenticationManager Starting");
				ADAuthManager = new ActiveDirectoryAuthenticationManager();
				ADAuthManager.Start();
				log.Info("ActiveDirectoryAuthenticationManager Started");
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				serviceHost.Open();

				log.Info("VoiceRecorderControllerService started successfully.");
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Error occured while starting VoiceRecorderControllerService.", ex);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (controller.CurrentDevice != null && controller.CurrentDevice.State != RecordingState.Stopped)
			{
				MessageBox.Show("Felvétel folyamatban, dobja el vagy mentse le.");
				e.Cancel = true;
			}
			base.OnClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			try
			{
				serviceHost.Close();
			}
			catch (Exception ex)
			{
				log.Error("Error occured while closing VoiceRecorderControllerService.", ex);
			}

			base.OnClosed(e);
			log.Info("Form Closed");
		}

		private void timerRetry_Tick(object sender, EventArgs e)
		{
			var changed = controller.DetectDeviceChanges();
			if (changed && controller.RecordingDevices.Count > 0) //hax to refresh cbDevice and cbQuality
			{
				cbDevice.SelectedIndex = -1;
				cbDevice.SelectedIndex = 0;
			}
		}

		private void gridRecordings_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete) return;
			int idx;
			var toDelete = GetSelectedItem(out idx);
			if (toDelete == null || toDelete.IsMarkedForDelete) return;
			var res = MessageBox.Show(this, "Biztosan törli a felvételt", "Törlés jóváhagyása", MessageBoxButtons.YesNo);
			if (res != DialogResult.Yes) return;
			controller.DeleteRecodring(toDelete);
		}

		private VoiceRecording GetSelectedItem(out int index)
		{
			index = -1;
			VoiceRecording item = null;
			if (gridRecordings.SelectedRows.Count > 0)
			{
				item = gridRecordings.SelectedRows[0].DataBoundItem as VoiceRecording;
			}
			else if (gridRecordings.SelectedCells.Count > 0)
			{
				item = gridRecordings.Rows[gridRecordings.SelectedCells[0].RowIndex].DataBoundItem as VoiceRecording;
			}
			if (item == null) return null;
			index = controller.VoiceRecordings.IndexOf(item);
			return item;
		}

		private void aboutToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			new AboutBox().Show(this);
		}

		private void openLogToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			try
			{
				var logFilePath = LogManager.GetRepository().GetAppenders().OfType<log4net.Appender.FileAppender>().First().File;
				Process.Start("notepad.exe", logFilePath);
			}
			catch (Exception ex)
			{
				log.Error("Unable to open log file", ex);
				//MessageBox.Show(Labels.NotificationCannotOpenLogBody, Labels.NotificationCannotOpenLogTitle, MessageBoxButtons.OK);
				MessageBox.Show("A naplófájl megnyitása sikertelen!", "Hiba!", MessageBoxButtons.OK);
			}
		}

		private void changeUserToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			var res = MessageBox.Show("Biztosan azonosítót vált? Ha igen, az alkalmazás bezárul, ezt követően ismét elindíthatja azt.", "Felhasználó azonosító váltás megerősítése.", MessageBoxButtons.YesNo);
			log.DebugFormat("Change user clicked, answer: {0}", res);
			if (res != DialogResult.Yes) return;
			LoginData.DeleteFromDisk();
			Close();
		}

		private void miStatusWindow_Click(object sender, EventArgs e)
		{
			miStatusWindow.Checked = !miStatusWindow.Checked;
		}

		private void UpdateStatusWindowVisibility()
		{
			if (miStatusWindow.Checked)
			{
				smallForm.Show();
				if (!Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(smallForm.Bounds))) //form is not on screen
				{
					var wa = Screen.PrimaryScreen.WorkingArea;
					smallForm.Location = new Point(wa.Left + 20, wa.Top + 20);
				}
			}
			else
			{
				smallForm.Hide();
			}
		}

		private void miStatusWindow_CheckedChanged(object sender, EventArgs e)
		{
			UpdateStatusWindowVisibility();
		}

		private void RecorderForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.Save();
			clientSettingsManager.SettingsChanged -= ClientSettingsManagerOnSettingsChanged;
			clientSettingsManager.Stop();
			log.Debug("ClientSettingsManager Stopped");
			versionReportManager.Stop();
			log.Debug("VersionReportManager Stopped");
			if (ADAuthManager != null)
			{
				ADAuthManager.Stop();
				log.Debug("ActiveDirectoryAuthenticationManager Stopped");
			}
		}

		private void miMandatoryName_Click(object sender, EventArgs e)
		{
			miMandatoryName.Checked = !miMandatoryName.Checked;
		}

		private void ClientSettingsManagerOnSettingsChanged(object sender, SingleValueEventArgs<ClientSetting> singleValueEventArgs)
		{
			guiContext.Post(_ => 
			{
				SetState(actualState);
				smallForm.UpdateSmallForm(actualState);
			}, null);
		}

		private void RefreshControls()
		{
			if (ConfigManager.IsNameMandatory.HasValue)
			{
				miMandatoryName.Checked = ConfigManager.IsNameMandatory.Value;
				miMandatoryName.Enabled = false;
			}
			else
			{
				miMandatoryName.Enabled = true;
			}

			if (ConfigManager.Quality.HasValue)
			{
				if (cbQuality.Items.Count > ConfigManager.Quality.Value)
				{
					cbQuality.SelectedIndex = ConfigManager.Quality.Value;
					log.DebugFormat("Quality changed to {0}", cbQuality.SelectedItem);
				}
				else
				{
					log.DebugFormat("Invalid quality setting ({0})", ConfigManager.Quality.Value);
				}
			}

			buttonsEnabled = ConfigManager.IsManualStartStopEnabled;
		}

		private void btnAgc_Click(object sender, EventArgs e)
		{
			isAgcOn = !isAgcOn;
			log.DebugFormat("Agc {0}", isAgcOn ? "On" : "Off");
			if (isAgcOn)
			{
				Debug.Assert(agc == null);
				agc = new AutoGainController(this);
				agc.Clear();
				btnAgc.Image = Properties.Resources.AgcOn;
			}
			else
			{
				agc = null;
				btnAgc.Image = Properties.Resources.AgcOff;
			}
		}

		public void SetVolume(int vol)
		{
			var volume = Math.Min(Math.Max(vol, 0), 100);
			guiContext.Post(_ =>
			{
				tbVolume.Value = volume;
			}, null);
		}

		public int GetVolume()
		{
			return tbVolume.Value;
		}
	}

}
