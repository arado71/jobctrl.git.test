using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.Maintenance;

namespace Tct.ActivityRecorderService.Voice
{
	public class VoiceRecordingPath : IFileCleanup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static VoiceRecordingPath instance = null;
		private static readonly object creationLock = new object();

		public static VoiceRecordingPath Instance
		{
			get
			{
				if (instance == null)
				{
					lock (creationLock)
					{
						if (instance == null)
						{
							instance = new VoiceRecordingPath();
						}
					}
				}

				return instance;
			}
		}

		public Maintenance.Storage Type
		{
			get
			{
				return Maintenance.Storage.Voice;
			}
		}

		private VoiceRecordingPath()
		{
		}

		public void Save(VoiceRecording recording)
		{
			var path = GetPath(recording);
			File.WriteAllBytes(path, recording.Data);
		}

		public string GetPath(VoiceRecording recording)
		{
			string dir, fileName;
			GetPath(recording, ConfigManager.VoiceRecordingsDir, out dir, out fileName);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			return Path.Combine(dir, fileName);
		}

		public ILookup<int, int> GetUserIds()
		{
			var result = new List<Tuple<int, int>>();
			List<string> directories;
			try
			{
				directories = Directory.EnumerateDirectories(ConfigManager.VoiceRecordingsDir).ToList();
			}
			catch (IOException ex)
			{
				log.Warn("Failed to enumerate directories in " + ConfigManager.VoiceRecordingsDir, ex);
				return result.ToLookup(k => k.Item1, v => v.Item2);
			}
			foreach (var dir in directories)
			{
				if (!int.TryParse(Path.GetFileName(dir), out var companyId)) continue;
				try
				{
					var subDirectories = Directory.EnumerateDirectories(dir).ToList();
					foreach (var subDir in subDirectories)
					{
						if (!int.TryParse(Path.GetFileName(subDir), out var userId)) continue;
						result.Add(Tuple.Create(companyId, userId));
					}
				}
				catch (IOException ex)
				{
					log.Warn("Failed to enumerate subdirectories in " + dir, ex);
				}
			}

			return result.ToLookup(k => k.Item1, v => v.Item2);
		}

		public IEnumerable<string> GetPaths(int companyId, int? userId)
		{
			var path = Path.Combine(ConfigManager.VoiceRecordingsDir, companyId.ToString(CultureInfo.InvariantCulture));
			return new[] { userId.HasValue ? Path.Combine(path, userId.Value.ToString(CultureInfo.InvariantCulture)) : path };
		}

		private void GetPath(VoiceRecording recording, string root, out string dir, out string fileName)
		{
			if (recording == null) throw new ArgumentNullException("recording");

			fileName = recording.UserId
				+ "_" + recording.StartDate.ToString("HH-mm-ss")
				+ "_" + recording.Id
				+ "." + recording.Extension;

			var subdirs = Path.Combine(root, recording.CompanyId.ToString());
			subdirs = Path.Combine(subdirs, recording.UserId.ToString());
			subdirs = Path.Combine(subdirs, recording.StartDate.ToString("yyyy-MM-dd"));
			dir = subdirs;
		}
	}
}
