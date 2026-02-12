using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public class ImageStorageCleaner
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ImageStorageCleaner));

		public static void CleanStoredImages(bool inDataFolder = false)
		{
			Stopwatch sw = Stopwatch.StartNew();
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			var imageDict = inDataFolder ? SortImagesByRule(IsolatedStorageHelper.ContentedItems) : SortImagesByRule(IsolatedStorageHelper.GetImages);
			int deleted = 0;
			int totalImages = 0;
			foreach (var imageSet in imageDict)
			{
				var imageStore = new Dictionary<byte[], IsolatedStorageHelper.ImageDataExt>(new HashComparer());
				foreach (var img in imageSet.Value)
				{
					byte[] hash = md5.ComputeHash(img.Imagedata);
					if (imageStore.ContainsKey(hash))
					{
						var previous = imageStore[hash];
						using (Bitmap one = new Bitmap(previous.Image), other = new Bitmap(img.Image))
						{
							if (ImageEquals(one, other))
							{
								if (inDataFolder)
								{
									IsolatedStorageHelper.Delete(img.Guid);
								}
								else
								{
									IsolatedStorageHelper.Delete(img.FileName);
								}
								
								deleted++;
							}
							else
							{
								log.DebugFormat("Different images with the same hash!");
							}
						}
					}
					else
					{
						imageStore.Add(hash, img);
					}
					totalImages++;
				}
			}
			sw.Stop();
			log.DebugFormat("Storage cleaning finished in {0} second(s). {1} images deleted from {2} in {3} folder", sw.Elapsed.TotalSeconds, deleted, totalImages, inDataFolder ? "data" : "base");
		}

		private static Dictionary<int, HashSet<IsolatedStorageHelper.ImageDataExt>> SortImagesByRule(IEnumerable<IsolatedStorageHelper.ImageDataExt> images)
		{
			var sortedImages = new Dictionary<int, HashSet<IsolatedStorageHelper.ImageDataExt>>();
			foreach (var img in images)
			{
				int ruleId = img.RuleId;
				if (!sortedImages.ContainsKey(ruleId))
				{
					sortedImages.Add(ruleId, new HashSet<IsolatedStorageHelper.ImageDataExt>());
				}
				sortedImages[ruleId].Add(img);
			}
			return sortedImages;
		}

		private static Dictionary<int, HashSet<IsolatedStorageHelper.ImageDataExt>> SortImagesByRule(IEnumerable<Snippet> images)
		{
			var sortedImages = new Dictionary<int, HashSet<IsolatedStorageHelper.ImageDataExt>>();
			foreach (var img in images)
			{
				int ruleId = img.RuleId;
				if (!sortedImages.ContainsKey(ruleId))
				{
					sortedImages.Add(ruleId, new HashSet<IsolatedStorageHelper.ImageDataExt>());
				}
				sortedImages[ruleId].Add(new IsolatedStorageHelper.ImageDataExt(img.ImageData, img.RuleId, img.ProcessName, img.Content) { Guid = img.Guid});
			}
			return sortedImages;
		}

		public static bool ImageEquals(Bitmap one, Bitmap other)
		{
			Debug.Assert(other != null);
			Debug.Assert(one != null);

			if ((other == null) != (one == null)) return false;
			if (other.Size != one.Size) return false;

			unsafe
			{
				var rectBase = new Rectangle(0, 0, one.Width, one.Height);
				var rectOther = new Rectangle(0, 0, other.Width, other.Height);
				BitmapData resultData = null, expectedData = null;
				if (rectBase != rectOther)
					return false;
				try
				{
					resultData = one.LockBits(rectBase, ImageLockMode.ReadOnly, one.PixelFormat);
					expectedData = other.LockBits(rectOther, ImageLockMode.ReadOnly, other.PixelFormat);
					return WinApi.memcmp(resultData.Scan0, expectedData.Scan0, resultData.Stride * resultData.Height) == 0;
				}
				finally
				{
					one.UnlockBits(resultData);
					other.UnlockBits(expectedData);
				}
			}
		}

		private class HashComparer : IEqualityComparer<byte[]>
		{
			public bool Equals(byte[] x, byte[] y)
			{
				return x.SequenceEqual(y);
			}

			public int GetHashCode(byte[] obj)
			{
				int sum = 0;
				obj.ToList().ForEach((x) => sum += x);		
				return sum;
			}
		}
	}
}
