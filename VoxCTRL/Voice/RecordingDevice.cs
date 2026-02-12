using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using NAudio.Mixer;
using NAudio.Wave;
using VoxCTRL.Serialization;
using VoxCTRL.Voice.Codecs;

namespace VoxCTRL.Voice
{
	public class RecordingDevice : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int Id { get; set; }
		public string Name { get; set; }
		public int Channels { get; set; }
		public WaveFormatData WaveFormat { get; set; }
		public int BytesWritten { get { return totalBytesRecorded; } }
		public TimeSpan RecordingTime
		{
			get
			{
				return prevRecordTime + ((stopTime == DateTime.MinValue ? DateTime.Now : stopTime) - startTime);
			}
		}
		private double volume;
		public double Volume
		{
			get { return volume; }
			set
			{
				volume = value;
				if (volumeControl != null)
				{
					try
					{
						volumeControl.Percent = value;
					}
					catch (Exception ex)
					{
						log.Debug("Cannot change volume", ex);
					}
				}
			}
		}

		private RecordingState currentState;
		public RecordingState State
		{
			get { return currentState; }
			private set
			{
				currentState = value;
				log.Info("State of " + Name + " changed to " + value);
				OnStateChanged(value);
			}
		}

		public event EventHandler<RecordingDeviceEventArgs> DataAvailable;
		public event EventHandler<RecordingEventArgs> EncodedDataAvailable;
		public event EventHandler<RecordingStateEventArgs> StateChanged;

		private readonly SampleAggregator sampleAggregator = new SampleAggregator();
		private TimeSpan prevRecordTime;
		private WaveIn waveIn;
		//private WaveFileWriter writer;
		private IEncoder encoder;
		private WaveFormatData recordingWaveFormat;
		private UnsignedMixerControl volumeControl;
		private DateTime startTime;
		private DateTime stopTime;
		private int totalBytesRecorded;

		public RecordingDevice()
		{
			WaveFormat = Mp3WaveFormatData.InstanceHi;
		}

		public override string ToString()
		{
			return Name + " (" + Channels + ")";
		}

		public bool StartRecording()
		{
			if (State != RecordingState.Stopped) return false;
			Debug.Assert(waveIn == null);
			recordingWaveFormat = WaveFormat;
			var naWaveFormat = recordingWaveFormat.GetWaveFormat();

			waveIn = new WaveIn();
			waveIn.DeviceNumber = Id;
			waveIn.WaveFormat = naWaveFormat;
			waveIn.DataAvailable += WaveDataAvailable;
			waveIn.RecordingStopped += WaveRecordingStopped;

			encoder = recordingWaveFormat.GetEncoder();

			sampleAggregator.NotificationCount = naWaveFormat.SampleRate / 10;
			sampleAggregator.Reset();
			sampleAggregator.SampleCalculated += OnSampleCalculated;
			prevRecordTime = TimeSpan.Zero;
			stopTime = DateTime.MinValue;
			startTime = DateTime.Now;
			totalBytesRecorded = 0;
			waveIn.StartRecording();
			TryGetVolumeControl();
			State = RecordingState.Recording;
			return true;
		}

		public IEnumerable<WaveFormatData> GetSupportedWaveFormats()
		{
			yield return Mp3WaveFormatData.InstanceHi;
			yield return Mp3WaveFormatData.InstanceMed;
			yield return Mp3WaveFormatData.InstanceLow;
			yield return GsmWaveFormatData.Instance;
			yield return LowStereoWaveFormatData.Instance;
			yield return HighStereoWaveFormatData.Instance;
		}

		public bool StopRecording()
		{
			if (State != RecordingState.Recording && State != RecordingState.Paused) return false;
			Debug.Assert(waveIn != null);
			waveIn.StopRecording();
			State = RecordingState.StopRequested;
			return true;
		}

		private void WaveRecordingStopped(object sender, StoppedEventArgs e)
		{
			if (e.Exception != null)
			{
				log.Error("Stopped recording due to unknow error", e.Exception);
			}
			if (stopTime == DateTime.MinValue) stopTime = DateTime.Now; //don't update if comming from Paused state
			var del = DataAvailable;
			if (del != null) del(this, new RecordingDeviceEventArgs(RecordingTime, 0, 0, totalBytesRecorded));

			waveIn.DataAvailable -= WaveDataAvailable;
			waveIn.RecordingStopped -= WaveRecordingStopped;
			sampleAggregator.SampleCalculated -= OnSampleCalculated;
			waveIn.Dispose();
			waveIn = null;

			var lastBytes = encoder.EncodeFlush();
			encoder.Dispose();
			encoder = null;

			OnEncodedDataAvailable(new RecordingEventArgs(RecordingTime, lastBytes ?? new byte[0], true));
			State = RecordingState.Stopped;
		}

		public bool PauseRecording()
		{
			if (State != RecordingState.Recording) return false;
			Debug.Assert(waveIn != null);
			//waveIn.StopRecording(); cannot start after stop...
			State = RecordingState.PauseRequested;
			stopTime = DateTime.Now;
			State = RecordingState.Paused;
			return true;
		}

		public bool ResumeRecording()
		{
			if (State != RecordingState.Paused) return false;
			Debug.Assert(waveIn != null);
			prevRecordTime = RecordingTime;
			stopTime = DateTime.MinValue;
			startTime = DateTime.Now;
			//waveIn.StartRecording();
			State = RecordingState.Recording;
			return true;
		}

		private void OnEncodedDataAvailable(RecordingEventArgs e)
		{
			var del = EncodedDataAvailable;
			if (del != null) del(this, e);
		}

		private void OnStateChanged(RecordingState state)
		{
			var del = StateChanged;
			if (del != null) del(this, new RecordingStateEventArgs(state));
		}

		private void OnSampleCalculated(object sender, SampleEventArgs e)
		{
			var del = DataAvailable;
			if (del != null) del(this, new RecordingDeviceEventArgs(RecordingTime, e.MinSample, e.MaxSample, totalBytesRecorded));
		}

		private void WaveDataAvailable(object sender, WaveInEventArgs e) //pcm data
		{
			Debug.Assert(encoder != null);
			if (State == RecordingState.Paused) return;
			byte[] buffer = e.Buffer;
			int bytesRecorded = e.BytesRecorded;

			if (encoder != null)
			{
				var encoded = encoder.EncodeBuffer(buffer, bytesRecorded);
				if (encoded != null)
				{
					OnEncodedDataAvailable(new RecordingEventArgs(RecordingTime, encoded, false));
				}
			}

			totalBytesRecorded += bytesRecorded;
			float left = 0;
			for (int index = 0; index < bytesRecorded; index += 2)
			{
				var pos = totalBytesRecorded - bytesRecorded + index;
				short sample = (short)((buffer[index + 1] << 8) |
										buffer[index + 0]);
				float sample32 = sample / 32768f;
				if (recordingWaveFormat.Channels == 2)
				{
					if (pos % 4 == 0)
					{
						left = sample32;
					}
					else if (pos % 4 == 2)
					{
						sampleAggregator.Add(left, sample32);
					}
				}
				else
				{
					sampleAggregator.Add(sample32);
				}
			}
		}

		private void TryGetVolumeControl()
		{
			int waveInDeviceNumber = waveIn.DeviceNumber;
			if (Environment.OSVersion.Version.Major >= 6) // Vista or later
			{
				var mixerLine = waveIn.GetMixerLine();
				foreach (var control in mixerLine.Controls)
				{
					if (control.ControlType == MixerControlType.Volume)
					{
						volumeControl = control as UnsignedMixerControl;
						try
						{
							Volume = volumeControl.Percent;
							break;
						}
						catch (Exception ex)
						{
							log.Error("Unable to set volume", ex);
						}
					}
				}
			}
			else
			{
				var mixer = new Mixer(waveInDeviceNumber);
				foreach (var destination in mixer.Destinations)
				{
					if (destination.ComponentType == MixerLineComponentType.DestinationWaveIn)
					{
						foreach (var source in destination.Sources)
						{
							if (source.ComponentType == MixerLineComponentType.SourceMicrophone)
							{
								foreach (var control in source.Controls)
								{
									if (control.ControlType == MixerControlType.Volume)
									{
										volumeControl = control as UnsignedMixerControl;
										try
										{
											Volume = volumeControl.Percent;
											break;
										}
										catch (Exception ex)
										{
											log.Error("Unable to set volume", ex);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		bool isDisposed;
		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			if (waveIn != null) waveIn.StopRecording();
		}
	}
}
