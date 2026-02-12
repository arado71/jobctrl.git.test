using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Controller
{
	public static class UniqueIdentityGenerator
	{
		private const string encodeTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private static readonly Random random = new Random();

		public static string Create(ulong id)
		{
			var rest = id;
			var encoded = new StringBuilder();
			var tableSize = (uint) encodeTable.Length;
			while (rest > 0)
			{
				var index = (int) (rest % tableSize);
				encoded.Append(encodeTable[index]);
				rest /= tableSize;
			}
			return encoded.ToString();
		}

		public static string Create()
		{
			byte[] buf = new byte[8];
			random.NextBytes(buf);
			ulong id = BitConverter.ToUInt64(buf, 0);
			return Create(id);

		}
	}
}
