using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tct.ActivityRecorderService;

namespace TelemetryConverter
{
	public static class Extension
	{
		public static StreamWriter GetStreamWriter(this HttpListenerContext context)
		{
			return new StreamWriter(context.Response.OutputStream, Encoding.UTF8);
		}

		public static void SetResponse(this HttpListenerContext context, string content)
		{
			context.Response.Headers.Add("Content-Type: text/html; charset=utf-8");
			using (var stream = context.GetStreamWriter())
			{
				stream.Write(content);
			}
		}

		public static void SetJsonResponse(this HttpListenerContext context, object responseObject)
		{
			context.Response.Headers.Add("Content-Type: application/javascript");
			using (var stream = context.GetStreamWriter())
			{
				stream.Write(JsonConvert.SerializeObject(responseObject));
			}
		}

		public static void SetResponse(this HttpListenerContext context, JObject json)
		{
			context.Response.Headers.Add("Content-Type: application/javascript");
			using (var stream = context.GetStreamWriter())
			{
				stream.Write(json.ToString());
			}
		}

		public static JObject GetRequestJson(this HttpListenerContext context)
		{
			using (var stream = new StreamReader(context.Request.InputStream))
			{
				var content = stream.ReadToEnd();
				return JObject.Parse(content);
			}
		}

		public static long ToUnixTime(this DateTime date)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((date - epoch).TotalMilliseconds);
		}

		public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
			Func<TValue> defaultValueFactory)
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
			{
				value = defaultValueFactory();
				dictionary.Add(key, value);
			}

			return value;
		}

		public static IntervalConcatenator Intersect(this IntervalConcatenator a, IntervalConcatenator b)
		{
			var res = a.Clone();
			var a1 = a.Clone();
			var b2 = b.Clone();
			a1.Subtract(b);
			b2.Subtract(a);
			res = res.Merge(b);
			res = res.Subtract(a1);
			res = res.Subtract(b2);
			return res;
		}

		public static IntervalConcatenator Intersect(this IntervalConcatenator a, DateTime startDate, DateTime endDate)
		{
			var other = new IntervalConcatenator();
			other.Add(startDate, endDate);
			return a.Intersect(other);
		}
	}
}
