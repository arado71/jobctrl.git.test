using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	public class RandomHelper
	{
		private static readonly RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();

		public static int Next(int maxValue)
		{
			if (maxValue < 1) throw new ArgumentOutOfRangeException("maxValue");
			var bytes = new byte[4];
			rnd.GetBytes(bytes);
			var validMaxValue = uint.MaxValue - (uint)(uint.MaxValue % maxValue);
			uint rndResult;
			do
			{
				rnd.GetBytes(bytes);
				rndResult = BitConverter.ToUInt32(bytes, 0);
			} while (rndResult >= validMaxValue);
			return (int)(rndResult % maxValue);
		}

	}
}
