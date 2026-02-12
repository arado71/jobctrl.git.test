using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VoxCTRL.ActivityRecorderServiceReference;

namespace VoxCTRL.Controller
{
	public class VoiceRecordingGenerator
	{
		private VoiceRecording currentRecording;
		private readonly byte[] buffer;
		private int offset;
		private int fileOffset;

		public event EventHandler<SingleValueEventArgs<VoiceRecording>> VoiceRecordingCreated;

		public VoiceRecordingGenerator()
			: this(64 * 1024)
		{
		}

		public VoiceRecordingGenerator(int maxChunkSize)
		{
			buffer = new byte[maxChunkSize];
		}

		public VoiceRecording StartNew(string name, int? workId, string extension, int codec)
		{
			Debug.Assert(currentRecording == null);
			fileOffset = 0;
			offset = 0;
			currentRecording = new VoiceRecording()
			{
				UserId = ConfigManager.UserId,
				StartDate = DateTime.UtcNow,
				Codec = codec,
				Extension = extension,
				ClientId = Guid.NewGuid(),
				Name = name,
				WorkId = workId,
			};
			return currentRecording.Clone(); //don't leak original
		}

		public void AddData(byte[] data, TimeSpan? duration, bool ended)
		{
			AddData(data, duration, ended, false);
		}

		public void ChangeData(string name)
		{
			if (currentRecording == null) return;
			currentRecording.Name = name;
		}

		private void AddData(byte[] data, TimeSpan? duration, bool ended, bool forceCreation)
		{
			Debug.Assert(currentRecording != null);
			var count = data.Length;
			do
			{
				var len = Math.Min(count, buffer.Length - offset);
				Array.Copy(data, data.Length - count, buffer, offset, len);
				offset += len;
				count -= len;
				if (count == 0 && duration.HasValue)
				{
					currentRecording.Duration = (int)duration.Value.TotalMilliseconds;
				}
				if ((ended && count == 0) || offset == buffer.Length || forceCreation) //done or buffer is full or forced
				{
					var dataToSend = currentRecording.Clone();
					if (ended && count == 0)
					{
						dataToSend.EndDate = DateTime.UtcNow;
						currentRecording = null;
					}
					dataToSend.Data = new byte[offset];
					dataToSend.Offset = fileOffset;
					Array.Copy(buffer, dataToSend.Data, offset);
					fileOffset += offset;
					offset = 0;
					OnVoiceRecordingCreated(dataToSend);
				}
			} while (count > 0);
		}

		public void FlushCurrent()
		{
			Debug.Assert(currentRecording != null);
			AddData(new byte[0], null, false, true);
		}

		private void OnVoiceRecordingCreated(VoiceRecording e)
		{
			var del = VoiceRecordingCreated;
			if (del != null) del(this, SingleValueEventArgs.Create(e));
		}
	}
}
