using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using log4net;

namespace Tct.MailFilterService
{
	[Serializable]
	public class DateStore
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public class item
		{
			[XmlAttribute]
			public string id;
			[XmlAttribute]
			public DateTime value;
		}

		public Dictionary<string, DateTime> Values { private set; get; }
		private bool Changed { set; get; }

		public void Load()
		{
			try
			{
				var serializer = new XmlSerializer(typeof(item[]));
				using (var reader = XmlReader.Create("lastdates.xml"))
				{
					Values = ((item[])serializer.Deserialize(reader)).ToDictionary(e => e.id, e => e.value);
				}
				Changed = false;
			}
			catch (FileNotFoundException)
			{
				return;
			}
			catch (Exception ex)
			{
				log.Error("Error during datestore saving");
				throw new Exception("loading date store");
			}
		}
		public void Save()
		{
			if (!Changed) return;
			try
			{
				var serializer = new XmlSerializer(typeof(item[]));
				using (var writer = XmlWriter.Create("lastdates.xml"))
				{
					serializer.Serialize(writer, Values.Select(kv => new item() { id = kv.Key, value = kv.Value }).ToArray());
				}
				Changed = false;
				log.Debug("Datestore saving completed");
			}
			catch (Exception ex)
			{
				log.Error("Error during datestore saving");
				throw new Exception("saving date store");
			}
		}

		public bool Exists(string key)
		{
			return Values != null && Values.Keys.Any(e => e == key);
		}
		public DateTime GetValue(string key)
		{
			return Exists(key) ? Values[key] : DateTime.UtcNow;
		}
		internal void Update(string p, DateTime mailDate)
		{
			if (Values == null)
				Values = new Dictionary<string, DateTime>();
			if (!Exists(p))
				Values.Add(p, mailDate);
			else
				Values[p] = mailDate;
			Changed = true;
		}
	}
}
