using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Tct.ActivityRecorderClient.Telemetry.Data;

namespace Tct.ActivityRecorderClient
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
            var settings = new DataContractJsonSerializerSettings
            {
                MaxItemsInObjectGraph = int.MaxValue,
                UseSimpleDictionaryFormat = true,
                IgnoreExtensionDataObject = false,
                KnownTypes = new List<Type>
                {
                    typeof(FeatureData),
                    typeof(ExceptionData),
                    // Add more known types here
                }
            };

            var ser = new DataContractJsonSerializer(typeof(T), settings);
            using (var stream = new MemoryStream())
            {
                ser.WriteObject(stream, data);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
