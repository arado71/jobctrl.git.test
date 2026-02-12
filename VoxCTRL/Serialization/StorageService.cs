using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using VoxCTRL.ActivityRecorderServiceReference;
using log4net;

namespace VoxCTRL.Serialization
{
	public class StorageService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string tempDir = "Temp";
		private const string voiceListDir = "VoiceLists";
		private const string voiceUpload = "VoiceUpload";
		private readonly string rootDir;

		public StorageService()
		{
			var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigManager.ApplicationName);
			rootDir = Path.Combine(dir, ConfigManager.UserId.ToString());
			Directory.CreateDirectory(rootDir);
			log.Info("Using the following root dir " + rootDir);
		}

		public string GetTempFilePath()
		{
			var currTempDir = Path.Combine(rootDir, tempDir);
			Directory.CreateDirectory(currTempDir);
			return Path.Combine(currTempDir, Path.GetRandomFileName());
		}

		public byte[] ReadAllBytes(string path)
		{
			return File.ReadAllBytes(path);
		}

		public void Delete(string path)
		{
			File.Delete(path);
		}

		public bool Exists(string path)
		{
			return File.Exists(path);
		}

		public void Delete(VoiceRecording voiceRecording)
		{
			Delete(GetPath(voiceRecording));
		}

		public bool Exists(VoiceRecording voiceRecording)
		{
			return Exists(GetPath(voiceRecording));
		}

		public void Save(VoiceRecording itemToSave)
		{
			itemToSave = itemToSave.Clone(); //Clone to avoid event serialization
			var path = GetPath(itemToSave);
			using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, itemToSave);
			}
		}

		public void SaveVoiceRecordings(IEnumerable<VoiceRecording> itemsToSave, string name) //todo use Replace
		{
			var itemToSave = itemsToSave.Select(n => n.Clone()).ToArray(); //Clone ! i.e.: don't serialize delegates
			var currDir = GetVoiceListDir();
			var path = Path.Combine(currDir, name);
			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, itemToSave);
			}
		}

		public VoiceRecording[] LoadVoiceRecordings(string name)
		{
			var currDir = GetVoiceListDir();
			var path = Path.Combine(currDir, name);
			if (!File.Exists(path)) return new VoiceRecording[0];
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				IFormatter formatter = new BinaryFormatter();
				return (VoiceRecording[])formatter.Deserialize(stream);
			}
		}

		public void Update(VoiceRecording itemToSave) //todo use Replace
		{
			itemToSave = itemToSave.Clone(); //Clone to avoid event serialization
			var path = GetPath(itemToSave);
			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, itemToSave);
			}
		}

		public VoiceRecording Load(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				if (stream.Length == 0)
				{
					log.Error("Deleting empty item from path: " + path);
					try
					{
						stream.Close();
						Delete(path);
						return null;
					}
					catch (Exception ex)
					{
						log.Error("Unable to delete empty item from path: " + path, ex);
						return null;
					}
				}
				IFormatter formatter = new BinaryFormatter();
				return (VoiceRecording)formatter.Deserialize(stream);
			}
		}

		private string GetVoiceUploadDir()
		{
			var result = Path.Combine(rootDir, voiceUpload);
			Directory.CreateDirectory(result);
			return result;
		}

		public double GetVoiceUploadDirSize()
		{
			var dir = new DirectoryInfo(GetVoiceUploadDir());
			long size = 0;
			FileInfo[] fis = dir.GetFiles();
			foreach (FileInfo fi in fis)
			{
				size += fi.Length;
			}
			return size / 1000000.0;
		}

		private string GetVoiceListDir()
		{
			var result = Path.Combine(rootDir, voiceListDir);
			Directory.CreateDirectory(result);
			return result;
		}

		public string GetPath(VoiceRecording voiceRecording)
		{
			return Path.Combine(GetVoiceUploadDir(), "u" + voiceRecording.UserId + "_" + voiceRecording.StartDate.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + voiceRecording.ClientId + "_" + voiceRecording.Offset + ".rec");
		}

		public List<string> GetVoiceRecordingPaths()
		{
			return Directory.GetFiles(GetVoiceUploadDir(), "*.rec").Select(n => Path.Combine(GetVoiceUploadDir(), n)).ToList();
		}
	}
}
