using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Serialization
{
	public static class GuidHelper
	{
		public static Guid IncreaseGuid(Guid guid) //for alphabetic ordering
		{
			var bytes = guid.ToByteArray();
			var carry = true;
			var idx = bytes.Length;
			while (--idx >= 0 && carry)
			{
				carry = bytes[idx] == byte.MaxValue;
				unchecked
				{
					bytes[idx]++;
				}
			}
			return new Guid(bytes);
		}
	}
}
