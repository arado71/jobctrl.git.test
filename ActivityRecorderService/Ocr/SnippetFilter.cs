using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using log4net;

namespace Tct.ActivityRecorderService.Ocr
{
	public class SnippetFilter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Dictionary<int, Dictionary<byte[], Guid>> storage;
		private readonly MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

		public SnippetFilter()
		{
			storage = new Dictionary<int, Dictionary<byte[], Guid>>();
			LoadSnippets();
		}

		public bool IsDuplicate(Snippet snippet)
		{
			int ruleId = snippet.RuleId;
			if (!storage.ContainsKey(ruleId))
			{
				storage.Add(ruleId, new Dictionary<byte[], Guid>(new HashComparer()));
			}

			byte[] hash = md5.ComputeHash(snippet.ImageData);
			if (storage[ruleId].ContainsKey(hash))
			{
				var previous = GetSnippet(storage[ruleId][hash]);
				if (previous == null)
				{
					log.DebugFormat("Missing snippet (Guid: {0})", storage[ruleId][hash]);
					storage[ruleId].Remove(hash);
				}
				else
				{
					using (Bitmap one = new Bitmap(ByteArrayToImage(previous.ImageData)), other = new Bitmap(ByteArrayToImage(snippet.ImageData)))
					{
						if (ImageEquals(one, other))
						{
							HandleSameSnippet(previous, snippet.Content);
							return true;
						}
						log.DebugFormat("Different images with the same hash!");
						return false;
					}
				}
			}
			storage[ruleId].Add(hash, snippet.Guid);
			return false;
		}

		private void LoadSnippets()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				foreach (var snippet in context.Snippets)
				{
					if (!storage.ContainsKey(snippet.RuleId))
					{
						storage.Add(snippet.RuleId, new Dictionary<byte[], Guid>(new HashComparer()));
					}
					byte[] hash = md5.ComputeHash(snippet.ImageData);
					try
					{
						storage[snippet.RuleId].Add(hash, snippet.Guid);
					}
					catch (ArgumentException ex)
					{
						log.DebugFormat("Multiple snippets with the same hash: {0} and {1}", storage[snippet.RuleId][hash], snippet.Guid);
					}
				}
			}
		}

		private Image ByteArrayToImage(byte[] imageData)
		{
			using (MemoryStream ms = new MemoryStream(imageData))
				return Image.FromStream(ms);
		}

		private static Snippet GetSnippet(Guid guid)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var snippet = context.Snippets.FirstOrDefault(s => s.Guid == guid);
				return snippet;
			}
		}

		private void HandleSameSnippet(Snippet baseSnippet, string newContent)
		{
			if (baseSnippet.Quality >= 5) return;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var originalSnippet = context.Snippets.FirstOrDefault(s => s.Guid == baseSnippet.Guid);
				if (originalSnippet == null) return;
				originalSnippet.ProcessedAt = null;
				originalSnippet.Content = newContent;
				context.SubmitChanges();
			}
		}

		private static bool ImageEquals(Bitmap one, Bitmap other)
		{
			Debug.Assert(other != null);
			Debug.Assert(one != null);

			if ((other == null) != (one == null)) return false;
			if (other.Size != one.Size) return false;

			var rectBase = new Rectangle(0, 0, one.Width, one.Height);
			var rectOther = new Rectangle(0, 0, other.Width, other.Height);
			BitmapData resultData = null, expectedData = null;
			if (rectBase != rectOther)
				return false;
			try
			{
				resultData = one.LockBits(rectBase, ImageLockMode.ReadOnly, one.PixelFormat);
				expectedData = other.LockBits(rectOther, ImageLockMode.ReadOnly, other.PixelFormat);

				int bytes = one.Width * one.Height * (Image.GetPixelFormatSize(one.PixelFormat) / 8);
				byte[] b1Bytes = new byte[bytes];
				byte[] b2Bytes = new byte[bytes];

				Marshal.Copy(resultData.Scan0, b1Bytes, 0, bytes);
				Marshal.Copy(expectedData.Scan0, b2Bytes, 0, bytes);

				for (int n = 0; n <= bytes - 1; n++)
				{
					if (b1Bytes[n] != b2Bytes[n])
					{
						return false;
					}
				}
				return true;
			}
			finally
			{
				one.UnlockBits(resultData);
				other.UnlockBits(expectedData);
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
