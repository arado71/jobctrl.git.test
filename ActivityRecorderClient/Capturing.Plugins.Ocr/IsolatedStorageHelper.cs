using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public static class IsolatedStorageHelper
	{
		private const string StoreDir = "Snippets";
		private static string rootPath { get { return StoreDir + "-" + ConfigManager.UserId; } }			// store images
		private static string dataPath { get { return StoreDir + "-" + ConfigManager.UserId + "\\data"; } }

		static IsolatedStorageHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(rootPath);
			IsolatedStorageSerializationHelper.CreateDir(dataPath);
		}
		public static void Save(Bitmap data, int ruleId, string processName, string contentGuess)
		{
			new ImageData(data, ruleId, processName, contentGuess).Save();
		}

		public static bool Save(Snippet data)
		{
			return IsolatedStorageSerializationHelper.Save(Path.Combine(dataPath, data.Guid.ToString()), data);
		}
		public static void Delete(Guid guid)
		{
			string fullPath = Path.Combine(dataPath, guid.ToString());
			IsolatedStorageSerializationHelper.Delete(fullPath);
		}
		public static void Delete(string fileName)
		{
			string fullPath = Path.Combine(rootPath, fileName);
			IsolatedStorageSerializationHelper.Delete(fullPath);
		}
		public static void DeleteDirectory(string dir)
		{
			IsolatedStorageSerializationHelper.DeleteDirectory(dir);
		}
		public static void SwipeOutdatedFiles(int olderThanDays)		// probably a negative number
		{
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(rootPath, "*")))
			{
				FileInfo fi = new FileInfo(fileName);
				if (fi.CreationTime < DateTime.Now.AddDays(olderThanDays))
					IsolatedStorageSerializationHelper.Delete(fileName);
			}
		}
		public static IEnumerable<ImageDataExt> GetImages
		{
			get
			{
				foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(rootPath, "*")))
				{
					ImageData loaded;
					if (IsolatedStorageSerializationHelper.Load(Path.Combine(rootPath, fileName), out loaded))
						yield return new ImageDataExt(loaded.Imagedata, loaded.RuleId, loaded.ProcessName, loaded.ContentGuess) { FileName = fileName };
				}
			}
		}
		public static IEnumerable<Snippet> ContentedItems
		{
			get
			{
				foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(dataPath, "*")))
				{
					Snippet loaded;
					if (IsolatedStorageSerializationHelper.Load(Path.Combine(dataPath, fileName), out loaded))
					{
						loaded.ProcessedAt = null;
						yield return loaded;
					}
				}
			}
		}
		[DataContract]
		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public class ImageData
		{
			public byte[] Imagedata { get; set; }
			public int RuleId { get; private set; }
			public string ProcessName { get; private set; }
			public string ContentGuess { get; set; }
			[IgnoreDataMember]
			public Image Image
			{
				get { return byteArrayToImage(Imagedata); }
			}

			public ImageData(byte[] imageData, int ruleId, string processName, string contentGuess)
			{
				this.Imagedata = imageData;
				this.RuleId = ruleId;
				this.ProcessName = processName;
				this.ContentGuess = contentGuess;
			}
			public ImageData(Image image, int ruleId, string processName, string contentGuess)
			{
				this.Imagedata = imageToByteArray(image);
				this.RuleId = ruleId;
				this.ProcessName = processName;
				this.ContentGuess = contentGuess;
			}
			public void Save()
			{
				if (Imagedata == null || RuleId == 0) return;
				var file = Path.GetRandomFileName();
				IsolatedStorageSerializationHelper.Save(Path.Combine(rootPath, file), this);
			}
			private Image byteArrayToImage(byte[] imageData)
			{
				using (MemoryStream ms = new MemoryStream(imageData))
					return System.Drawing.Image.FromStream(ms);
			}
			private byte[] imageToByteArray(Image image)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					image.Save(ms, ImageFormat.Png);
					return ms.ToArray();
				}
			}
		}
		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public class ImageDataExt : ImageData
		{
			public ImageDataExt(byte[] imageData, int ruleId, string processName, string contentGuess) : base(imageData, ruleId, processName, contentGuess) { }
			public string FileName { get; set; }
			public Guid Guid { get; set; }
		}
	}
}
