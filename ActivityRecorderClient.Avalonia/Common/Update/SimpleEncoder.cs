using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Update
{
	public static class SimpleEncoder
	{
		public static string Encode(string input)
		{
			try
			{
				var bytes = Encoding.Default.GetBytes(input);
				MaskBytes(bytes);
				return Convert.ToBase64String(bytes);
			}
			catch
			{
				return null;
			}
		}

		private static void MaskBytes(byte[] bytes)
		{
			var mask = (byte) bytes.Length;
			for (int i = 0; i < bytes.Length; i++)
			{
				var b = bytes[i];
				bytes[i] = (byte) (b ^ mask);
			}
		}

		public static string Decode(string input)
		{
			try
			{
				var bytes = Convert.FromBase64String(input);
				MaskBytes(bytes);
				return Encoding.Default.GetString(bytes);
			}
			catch
			{
				return null;
			}
		}
	}
}
