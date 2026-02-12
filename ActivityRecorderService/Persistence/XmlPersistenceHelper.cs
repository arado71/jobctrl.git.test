using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Tct.ActivityRecorderService.Persistence
{
	public static class XmlPersistenceHelper
	{
		internal static Func<string, Stream> FileWriteStreamFactory = GetFileWriteStream;
		internal static Func<string, Stream> FileReadStreamFactory = GetFileReadStream;

		public static void LoadFromFile<T>(string path, out T graph)
		{
			graph = XmlPersistenceHelper<T>.LoadFromFile(path);
		}

		public static void SaveToFile<T>(string path, T graph)
		{
			XmlPersistenceHelper<T>.SaveToFile(path, graph);
		}

		public static void WriteToStream<T>(Stream stream, T graph)
		{
			XmlPersistenceHelper<T>.WriteToStream(stream, graph);
		}

		public static void ReadFromStream<T>(Stream stream, out T graph)
		{
			graph = XmlPersistenceHelper<T>.ReadFromStream(stream);
		}

		internal static Stream GetFileReadStream(string path)
		{
			return File.OpenRead(path);
		}

		internal static Stream GetFileWriteStream(string path)
		{
			return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough | FileOptions.SequentialScan);
		}

		public static void LoadFromString<T>(string data, out T graph, Encoding encoding = null)
		{
			graph = XmlPersistenceHelper<T>.LoadFromString(data, encoding);
		}

		public static string SaveToString<T>(T graph, Encoding encoding = null)
		{
			return XmlPersistenceHelper<T>.SaveToString(graph, encoding);
		}
	}

	public static class XmlPersistenceHelper<T>
	{
		public static T LoadFromString(string data, Encoding encoding = null)
		{
			using (var stream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(data)))
			{
				return ReadFromStream(stream);
			}
		}

		public static string SaveToString(T graph, Encoding encoding = null)
		{
			using (var stream = new MemoryStream())
			{
				WriteToStream(stream, graph);
				return (encoding ?? Encoding.UTF8).GetString(stream.ToArray());
			}
		}

		public static T LoadFromFile(string path)
		{
			using (var fs = XmlPersistenceHelper.FileReadStreamFactory(path))
			{
				return ReadFromStream(fs);
			}
		}

		public static void SaveToFile(string path, T graph)
		{
			using (var fs = XmlPersistenceHelper.FileWriteStreamFactory(path))
			{
				WriteToStream(fs, graph);
			}
		}

		public static void WriteToStream(Stream stream, T graph, bool ignoreExtensionDataObject = false)
		{
			//using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize }))
			//using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize })))
			using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream))//no indent
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(T), null, Int32.MaxValue, ignoreExtensionDataObject, false, null);
				serializer.WriteObject(writer, graph);
			}
		}

		public static T ReadFromStream(Stream stream)
		{
			//using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings() { }))
			//using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas() { MaxDepth = 2001, }))
			using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(T));
				return (T)serializer.ReadObject(reader);
			}
		}
	}
}
