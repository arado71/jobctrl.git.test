using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Text;

namespace Tct.ActivityRecorderService.Telemetry
{
	[Serializable]
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TelemetryItem
	{
		[DataMember]
		public Dictionary<string, Dictionary<string, List<DateTime>>> EventNameValueOccurences { get; set; }
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public DateTime EndDate { get; set; }
		[DataMember]
		public int ComputerId { get; set; }
		[DataMember]
		public int UserId { get; set; }

		public void WriteTo(Stream stream)
		{
			using (var writer = new BinaryWriter(stream, Encoding.UTF8))
			{
				writer.Write(StartDate.Ticks);
				writer.Write(EndDate.Ticks);
				writer.Write(UserId);
				writer.Write(ComputerId);
				writer.Write(EventNameValueOccurences.Count);
				foreach (var eventNameValueOccurence in EventNameValueOccurences)
				{
					writer.Write(eventNameValueOccurence.Key);
					writer.Write(eventNameValueOccurence.Value.Count);
					foreach (var valueOccurence in eventNameValueOccurence.Value)
					{
						writer.Write(valueOccurence.Key);
						writer.Write(valueOccurence.Value.Count);
						foreach (var occurence in valueOccurence.Value)
						{
							writer.Write(occurence.Ticks);
						}
					}
				}
			}
		}

		public static TelemetryItem ReadFrom(Stream stream)
		{
			var result = new TelemetryItem();
			using (var reader = new BinaryReader(stream, Encoding.UTF8))
			{
				result.StartDate = new DateTime(reader.ReadInt64());
				result.EndDate = new DateTime(reader.ReadInt64());
				result.UserId = reader.ReadInt32();
				result.ComputerId = reader.ReadInt32();
				result.EventNameValueOccurences = new Dictionary<string, Dictionary<string, List<DateTime>>>();
				var eventNameCount = reader.ReadInt32();
				for (var i = 0; i < eventNameCount; ++i)
				{
					var currentValueOccurences = new Dictionary<string, List<DateTime>>();
					result.EventNameValueOccurences.Add(reader.ReadString(), currentValueOccurences);
					var paramCount = reader.ReadInt32();
					for (var j = 0; j < paramCount; ++j)
					{
						var paramName = reader.ReadString();
						var occurenceCount = reader.ReadInt32();
						var currentOccurences = new List<DateTime>(occurenceCount);
						currentValueOccurences.Add(paramName, currentOccurences);
						for (var k = 0; k < occurenceCount; ++k)
						{
							currentOccurences.Add(new DateTime(reader.ReadInt64()));
						}
					}
				}
			}

			return result;
		}
	}
}
