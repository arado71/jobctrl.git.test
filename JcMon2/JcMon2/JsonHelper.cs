using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JcMon2
{
	public static class JsonHelper
	{
		public static void DeserializeData<T>(string data, out T result)
		{
			var bytes = Encoding.UTF8.GetBytes(data);
			var ser = new DataContractJsonSerializer(typeof(T));
			using (var stream = new MemoryStream(bytes, false))
			{
				result = (T)ser.ReadObject(stream);
			}
		}

		public static string SerializeData<T>(T data)
		{
			var ser = new DataContractJsonSerializer(data.GetType(), (IEnumerable<Type>)null, int.MaxValue, true, null, false); //extension data is lost, but that is ok atm.
			using (var stream = new MemoryStream())
			{
				ser.WriteObject(stream, data);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
	}
}
