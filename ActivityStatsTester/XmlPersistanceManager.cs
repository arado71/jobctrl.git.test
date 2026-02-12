using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ActivityStatsTester
{
	public static class XmlPersistenceManager
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
			using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize }))
			//using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize })))
			//using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream))//no indent
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(T));
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
