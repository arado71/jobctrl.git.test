using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Tct.ActivityRecorderClient.Serialization
{
	public static class XmlSerializationHelper
	{
		public static void LoadFromFile<T>(string path, out T graph)
		{
			graph = XmlPersistenceManager<T>.LoadFromFile(path);
		}

		public static void SaveToFile<T>(string path, T graph)
		{
			XmlPersistenceManager<T>.SaveToFile(path, graph);
		}

		public static void WriteToStream<T>(Stream stream, T graph)
		{
			XmlPersistenceManager<T>.WriteToStream(stream, graph);
		}

		public static void ReadFromStream<T>(Stream stream, out T graph)
		{
			graph = XmlPersistenceManager<T>.ReadFromStream(stream);
		}

		public static bool AreTheSame<T>(T first, T second)
		{
			if (ReferenceEquals(first, null)) return (ReferenceEquals(second, null));
			if (ReferenceEquals(second, null)) return false;
			if (ReferenceEquals(first, second)) return true;
			using (MemoryStream ms1 = new MemoryStream())
			using (MemoryStream ms2 = new MemoryStream())
			{
				XmlPersistenceManager<T>.WriteToStream(ms1, first, true);
				XmlPersistenceManager<T>.WriteToStream(ms2, second, true);
				var arr1 = ms1.ToArray();
				var arr2 = ms2.ToArray();
				if (arr1.Length != arr2.Length) return false;
				return arr1.SequenceEqual(arr2);
			}
		}
	}

	public static class XmlPersistenceManager<T>
	{
		public static T LoadFromFile(string path)
		{
			using (FileStream fs = File.OpenRead(path))
			{
				return ReadFromStream(fs);
			}
		}

		public static void SaveToFile(string path, T graph)
		{
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				WriteToStream(fs, graph);
			}
		}

		public static void WriteToStream(Stream stream, T graph)
		{
			WriteToStream(stream, graph, false);
		}

		public static void WriteToStream(Stream stream, T graph, bool ignoreExtensionDataObject)
		{
			using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize }))
			//using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize })))
			//using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream))//no indent
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(T), null, Int32.MaxValue, ignoreExtensionDataObject, false, null);
				serializer.WriteObject(writer, graph);
			}
		}

		public static T ReadFromStream(Stream stream)
		{
			//using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings() { }))
			using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(T));
				return (T)serializer.ReadObject(reader);
			}
		}
	}
}
