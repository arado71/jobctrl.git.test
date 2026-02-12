using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using VoxCTRL.ActivityRecorderServiceReference;
using VoxCTRL.Serialization;
using VoxCTRL.Voice;

namespace VoxCTRL.Controller
{
	public class RecorderFormController : INotifyPropertyChanged, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly string gridFileName = "MainGrid";
#if DEBUG
		private const float tooSilentThreshold = 0.003f; // for too silent test
		private static readonly TimeSpan tooSilentNotificationTimeInterval = TimeSpan.FromMinutes(3); // 3mins
#else
		private const float tooSilentThreshold = 0.0005f;
		private static readonly TimeSpan tooSilentNotificationTimeInterval = TimeSpan.FromMinutes(60); // 60mins
#endif

		private readonly StorageService store;
		private readonly AudioService audio;
		private readonly BindingList<VoiceRecording> voiceRecordings = new BindingList<VoiceRecording>();
		private readonly VoiceRecordingGenerator voiceRecordingGenerator = new VoiceRecordingGenerator();
		private readonly VoiceRecordingUploadController uploadController = new VoiceRecordingUploadController();
		private readonly BindingList<RecordingDevice> recordingDevices = new BindingList<RecordingDevice>();
		private DateTime lastTooSilentNotficiationTime = DateTime.Now - tooSilentNotificationTimeInterval + TimeSpan.FromSeconds(15);
		private List<string> recPaths;

		public event EventHandler<RecordingDeviceEventArgs> DataAvailable;
		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler TooSilentRecordingNotification;
		public BindingList<RecordingDevice> RecordingDevices { get { return recordingDevices; } }
		public BindingList<VoiceRecording> VoiceRecordings { get { return voiceRecordings; } }

		private VoiceRecording currentRecording;
		public VoiceRecording CurrentRecording
		{
			get { return currentRecording; }
			private set { UpdateField(ref currentRecording, value, "CurrentRecording"); }
		}

		private RecordingDevice currentDevice;
		public RecordingDevice CurrentDevice
		{
			get { return currentDevice; }
			set
			{
				if (state != RecordingState.Stopped) return;
				if (currentDevice == value) return;
				UpdateField(ref currentDevice, value, "CurrentDevice");
				log.Info("CurrentDevice changed to " + (CurrentDevice != null ? CurrentDevice.ToString() : "(null)"));
				Debug.Assert(currentDevice == null || recordingDevices.Contains(currentDevice));
			}
		}

		private RecordingState state = RecordingState.Stopped;
		public RecordingState State
		{
			get { return state; }
			private set { UpdateField(ref state, value, "State"); }
		}

		private string title;
		public string Title
		{
			get { return title; }
			set { UpdateField(ref title, value, "Title"); }
		}

		private string recordingName;
		public string RecordingName
		{
			get { return recordingName ?? ""; }
			set
			{
				if (value == "") value = null;
				ChangeName(value);
				if (UpdateField(ref recordingName, value, "RecordingName"))
				{
					UpdateTitle();
				}
			}
		}

		private TimeSpan recordingTime;
		public TimeSpan RecordingTime
		{
			get { return recordingTime; }
			private set
			{
				value = TimeSpan.FromSeconds((int)value.TotalSeconds); //seconds precision, we don't want to raise this event too many times
				UpdateField(ref recordingTime, value, "RecordingTime");
			}
		}

		private bool isRecordingTooSilent;
		public bool IsRecordingTooSilent
		{
			get { return isRecordingTooSilent; }
			set { UpdateField(ref isRecordingTooSilent, value, "IsRecordingTooSilent"); }
		}

		public RecorderFormController()
			: this(new AudioService(), new StorageService())
		{
		}

		public RecorderFormController(AudioService audioService, StorageService storageService)
		{
			store = storageService;
			audio = audioService;
			DetectDeviceChanges();
			voiceRecordingGenerator.VoiceRecordingCreated += VoiceRecordingCreated;
			uploadController.VoiceRecordingUploaded += VoiceRecordingUploaded;
			UpdateTitle();
		}

		private static readonly TimeSpan fullCheckInterval = TimeSpan.FromMinutes(1);
		private DateTime lastFullCheck;
		public bool DetectDeviceChanges()
		{
			if (State != RecordingState.Stopped) return false; //we don't care about changes during recording atm.
			if (recordingDevices.Count == audio.RecordingDeviceCount && lastFullCheck + fullCheckInterval < DateTime.UtcNow) return false; //assume they are not changed
			lastFullCheck = DateTime.UtcNow;
			var newDevices = audio.GetRecordingDevices().ToList();
			if (recordingDevices.Select(n => n.ToString()).SequenceEqual(newDevices.Select(n => n.ToString()))) //they are the 'same' (at least their name is)
			{
				newDevices.ForEach(n => n.Dispose());
				return false;
			}

			CurrentDevice = null; //clear CurrentDevice
			foreach (var recordingDevice in recordingDevices)
			{
				log.Info("Unloading device " + recordingDevice);
				recordingDevice.Dispose();
				recordingDevice.DataAvailable -= DeviceDataAvailable;
				recordingDevice.EncodedDataAvailable -= EncodedDataAvailable;
				recordingDevice.StateChanged -= StateChanged;
			}
			recordingDevices.Clear(); //this will set selected index to -1 on recorder form
			foreach (var recordingDevice in newDevices)
			{
				log.Info("Loading device " + recordingDevice);
				recordingDevice.DataAvailable += DeviceDataAvailable;
				recordingDevice.EncodedDataAvailable += EncodedDataAvailable;
				recordingDevice.StateChanged += StateChanged;
				recordingDevices.Add(recordingDevice); //first add will set selected index to 0 but won't raise selectedindexchanged event (so we need a hax to handle this)
			}
			log.Info("Found " + recordingDevices.Count + " devices");
			return true;
		}

		private void UpdateTitle()
		{
			switch (State)
			{
				case RecordingState.Stopped:
					Title = "VoxCTRL";
					break;
				case RecordingState.Recording:
					Title = "Felvétel [" + RecordingName + "]";
					break;
				case RecordingState.Paused:
					Title = "Szünet [" + RecordingName + "]";
					break;
				default:
					return;
			}
		}

		private void VoiceRecordingCreated(object sender, SingleValueEventArgs<VoiceRecording> e)
		{
			PersistAndSend(e.Value);
		}

		private void PersistAndSend(VoiceRecording voiceRecording)
		{
			log.Info("Voice data created " + voiceRecording);
			store.Save(voiceRecording);
			uploadController.UploadRecordingAsync(voiceRecording);
		}

		private void VoiceRecordingUploaded(object sender, SingleValueEventArgs<VoiceRecording> e)
		{
			if (store.Exists(e.Value)) store.Delete(e.Value);
			log.Info("Voice data uploaded " + e.Value);
			if (!e.Value.IsMarkedForDelete && !e.Value.EndDate.HasValue) return;
			LoadOneVoiceRecording();
			var uploaded = VoiceRecordings.Where(n => n.ClientId == e.Value.ClientId).FirstOrDefault();
			if (uploaded == null) return;

			if (e.Value.EndDate.HasValue)
			{
				uploaded.EndDate = e.Value.EndDate;
				uploaded.Duration = e.Value.Duration;
				uploaded.Offset = e.Value.Offset + e.Value.Length; //hax set size
				if (uploaded.UploadDate == null) //not deleted already
				{
					uploaded.UploadDate = DateTime.UtcNow;
					uploaded.Status = "Feltöltve " + TimeZone.CurrentTimeZone.ToLocalTime(uploaded.UploadDate.Value).ToShortTimeString();
				}
			}

			if (e.Value.IsMarkedForDelete)
			{
				uploaded.UploadDate = DateTime.UtcNow;
				uploaded.IsMarkedForDelete = true;
				uploaded.Status = "Törölve " + TimeZone.CurrentTimeZone.ToLocalTime(uploaded.UploadDate.Value).ToShortTimeString();
			}
			SaveGrid();
		}

		private void DeviceDataAvailable(object sender, RecordingDeviceEventArgs e)
		{
			CurrentRecording.Duration = (int)e.RecordingTime.TotalMilliseconds;
			RecordingTime = e.RecordingTime;
			var isRecTooSilent = State == RecordingState.Recording && Math.Abs(e.MinVolume) < tooSilentThreshold && e.MaxVolume < tooSilentThreshold;
			if (isRecTooSilent != IsRecordingTooSilent)
			{
				IsRecordingTooSilent = isRecTooSilent;
				log.Info("Record volume is " + (IsRecordingTooSilent ? "too silent" : "ok"));
				if (isRecTooSilent && DateTime.Now - lastTooSilentNotficiationTime > tooSilentNotificationTimeInterval)
				{
					var handler = TooSilentRecordingNotification;
					if (handler != null)
						handler(this, EventArgs.Empty);
					lastTooSilentNotficiationTime = DateTime.Now;
				}
			}
			//todo save grid at intervals (1sec) ?
			var del = DataAvailable;
			if (del != null) del(sender, e);
		}

		private void StateChanged(object sender, RecordingStateEventArgs e)
		{
			State = e.State;
			Debug.Assert(CurrentRecording != null);
			if (CurrentRecording == null) return;
			if (e.State == RecordingState.Stopped) //last VoiceRecordingCreated is called when we got here
			{
				if (CurrentRecording.IsMarkedForDelete)
				{
					DeleteRecodring(CurrentRecording);
				}
				else
				{
					CurrentRecording.Status = "Felvétel befejezve";
				}
				CurrentRecording = null;
				RecordingName = ""; //reset RecordingName after CurrentRecording is nulled out
			}
			else if (e.State == RecordingState.Recording) CurrentRecording.Status = "Felvétel";
			else if (e.State == RecordingState.Paused) CurrentRecording.Status = "Szünet";
			UpdateTitle();
		}

		public void DeleteRecodring(VoiceRecording voiceRecording)
		{
			Debug.Assert(VoiceRecordings.Contains(voiceRecording));
			voiceRecording.IsMarkedForDelete = true;
			voiceRecording.UploadDate = null;
			voiceRecording.Status = "Törlés";
			SaveGrid();
			var dataToDelete = voiceRecording.Clone(); //hax
			//we don't upload audio data for delete
			dataToDelete.Data = null;
			dataToDelete.Offset = int.MaxValue; //hax
			PersistAndSend(dataToDelete);
		}

		private void EncodedDataAvailable(object sender, RecordingEventArgs e)
		{
			voiceRecordingGenerator.AddData(e.Data, e.RecordingTime, e.Ended);
		}

		public bool Record(string name)
		{
			if (CurrentDevice == null) return false;
			if (state != RecordingState.Stopped) return false;
			if (name == null) name = "";
			RecordingTime = TimeSpan.Zero;
			RecordingName = RecordingNameParser.Parse(name, new Dictionary<string, string> {{"UID", UniqueIdentityGenerator.Create()}, {"RECTIME", DateTime.UtcNow.ToString("yyyyMMddhhmmss")}});
			var curr = voiceRecordingGenerator.StartNew(RecordingName, null, CurrentDevice.WaveFormat.Extension, CurrentDevice.WaveFormat.CodecId);
			CurrentRecording = curr.Clone();
			CurrentRecording.Data = null; //we don't want data in the grid
			log.Info("Record " + CurrentRecording);
			VoiceRecordings.Insert(0, CurrentRecording);
			SaveGrid();
			CurrentDevice.StartRecording();
			return true;
		}

		public bool RecordOrResume()
		{
			if (CurrentDevice == null) return false;
			return CurrentDevice.State == RecordingState.Stopped ? Record(RecordingName) : Resume();
		}

		public bool Resume()
		{
			if (CurrentDevice == null) return false;
			if (state != RecordingState.Paused) return false;
			log.Info("Resume " + CurrentRecording);
			CurrentDevice.ResumeRecording();
			return true;
		}

		public bool Pause()
		{
			if (CurrentDevice == null) return false;
			if (state != RecordingState.Recording) return false;
			log.Info("Pause " + CurrentRecording);
			CurrentDevice.PauseRecording();
			return true;
		}

		public bool Stop(bool isForced = false)
		{
			if (CurrentDevice == null) return false;
			if (state == RecordingState.Stopped || state == RecordingState.StopRequested) return false;
			if (!isForced && Properties.Settings.Default.IsNameMandatory && string.IsNullOrEmpty(RecordingName)) return false;
			log.Info("Stop " + CurrentRecording);
			CurrentDevice.StopRecording();
			return true;
		}

		public bool StopAndDelete()
		{
			if (CurrentDevice == null) return false;
			if (state != RecordingState.Recording && state != RecordingState.Paused) return false;
			Debug.Assert(CurrentRecording != null);
			log.Info("StopAndDelete " + CurrentRecording);
			CurrentRecording.IsMarkedForDelete = true;
			CurrentDevice.StopRecording();
			return true;
		}

		private void ChangeName(string name)
		{
			if (CurrentRecording != null)
			{
				CurrentRecording.Name = name;
			}
			voiceRecordingGenerator.ChangeData(name);
		}

		private void SaveGrid()
		{
			store.SaveVoiceRecordings(VoiceRecordings, gridFileName);
		}

		private void LoadOneVoiceRecording()
		{
			if (recPaths.Count == 0) return;
			var recsToUpload = new List<VoiceRecording>();
			var firstRec = store.Load(recPaths.First());
			foreach (var recPath in recPaths.Where(r => r.Contains(firstRec.ClientId.ToString())))
			{
				try
				{
					var rec = store.Load(recPath);
					if (rec.ClientId == Guid.Empty) continue; //old format
					Debug.Assert(!rec.UploadDate.HasValue);
					recsToUpload.Add(rec);
				}
				catch (Exception ex)
				{
					log.Error("Unable to load recording from " + recPath, ex);
				}
			}
			recPaths.RemoveAll(r => r.Contains(firstRec.ClientId.ToString()));

			foreach (var recToUpload in recsToUpload.OrderBy(n => n.StartDate).ThenBy(n => n.ClientId).ThenBy(n => n.Offset)) //StartDate, ClientId should be the same for all 'groupped' recordings
			{
				uploadController.UploadRecordingAsync(recToUpload);
			}
		}

		public void LoadRecordings()
		{
			log.Info("Loading previous recordings");
			recPaths = store.GetVoiceRecordingPaths();
			var recCount = recPaths.Count;

			try
			{
				var grid = store.LoadVoiceRecordings(gridFileName);
				const int maxSize = 200;
				if (grid.Length > maxSize) log.Info("Skip loading " + (grid.Length - maxSize) + " recordings");
				foreach (var rec in grid.Take(maxSize))
				{
					if (rec.IsMarkedForDelete)
					{
						rec.Status = rec.UploadDate == null ? "Törlés" : "Törölve " + TimeZone.CurrentTimeZone.ToLocalTime(rec.UploadDate.Value).ToShortTimeString();
					}
					else
					{
						rec.Status = rec.UploadDate == null ? "Felvétel befejezve" : "Feltöltve " + TimeZone.CurrentTimeZone.ToLocalTime(rec.UploadDate.Value).ToShortTimeString();
					}
					if (rec.UploadDate == null) //since we don't save grid on duration change get info from saved data
					{
						rec.Duration = -1;
					}

					VoiceRecordings.Add(rec);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to load grid data", ex);
			}

			LoadOneVoiceRecording();
			log.InfoFormat("Loaded {0} recordings with {1} items ({2} MB) to upload", voiceRecordings.Count, recCount, store.GetVoiceUploadDirSize());
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var propChanged = PropertyChanged;
			if (propChanged != null) propChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool UpdateField<T>(ref T field, T value, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		public void Dispose()
		{
			if (CurrentDevice != null && CurrentDevice.State != RecordingState.Stopped)
			{
				//we should only dispose and unsubscribe from recodingDevice if State == RecordingState.Stopped fortunately that is the case (otherwise we could lose some events)
				log.ErrorAndFail("Disposed in invalid state: " + CurrentDevice.State);
			}
			if (CurrentDevice != null && State != RecordingState.Stopped && State != RecordingState.StopRequested) CurrentDevice.StopRecording();
			foreach (var recordingDevice in recordingDevices)
			{
				recordingDevice.Dispose();
				recordingDevice.DataAvailable -= DeviceDataAvailable;
				recordingDevice.EncodedDataAvailable -= EncodedDataAvailable;
				recordingDevice.StateChanged -= StateChanged;
			}
			uploadController.Dispose();
		}
	}
}
