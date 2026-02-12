using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Maintenance
{
	public enum Storage
	{
		Screenshot,
		Mobile,
		Log,
		Voice,
		Telemetry,
		None
	}

	public class FileCleanupSection : ConfigurationSection
	{
		[ConfigurationProperty("limits", IsRequired = true)]
		public LimitElementCollection Limits
		{
			get
			{
				return base["limits"] as LimitElementCollection;
			}

			set
			{
				base["limits"] = value;
			}
		}

		public LimitElement GetApplicableElement(int companyId, Storage storage)
		{
			var res = Limits.Cast<LimitElement>().FirstOrDefault(x => x.CompanyId == companyId && x.Storage == storage);
			if (res != null) return res;
			res = Limits.Cast<LimitElement>().FirstOrDefault(x => x.CompanyId == companyId && x.Storage == Storage.None);
			if (res != null) return res;
			res = Limits.Cast<LimitElement>().FirstOrDefault(x => x.CompanyId == null && x.Storage == storage);
			if (res != null) return res;
			return Limits.Cast<LimitElement>().FirstOrDefault(x => x.CompanyId == null && x.Storage == Storage.None);
		}
	}

	public class LimitElement : ConfigurationElement
	{
		[ConfigurationProperty("companyId", DefaultValue = null, IsRequired = false)]
		public int? CompanyId
		{
			get
			{
				return (int?) this["companyId"];
			}

			set
			{
				this["companyId"] = value;
			}
		}

		[ConfigurationProperty("storage", DefaultValue = Storage.None, IsRequired = false)]
		public Storage Storage
		{
			get
			{
				return (Storage) this["storage"];
			}

			set
			{
				this["storage"] = value;
			}
		}

		[ConfigurationProperty("maxSize", DefaultValue = null, IsRequired = false)]
		public string MaxSize
		{
			get
			{
				return (string) this["maxSize"];
			}

			set
			{
				this["maxSize"] = value;
			}
		}

		[ConfigurationProperty("maxAge", DefaultValue = null, IsRequired = false)]
		public string MaxAge
		{
			get
			{
				return (string) this["maxAge"];
			}

			set
			{
				this["maxAge"] = value;
			}
		}

		public int? UserId { get; set; }

		public string Key => $"{CompanyId?.ToString() ?? ""}_{UserId?.ToString() ?? ""}_{(int)Storage}_{MaxSize ?? ""}_{MaxAge ?? ""}";
	}

	[ConfigurationCollection(typeof(LimitElement), AddItemName = "limit")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class LimitElementCollection : ConfigurationElementCollection
	{
		public void Add(LimitElement element)
		{
			LockItem = false;
			BaseAdd(element, false);
			LockItem = true;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new LimitElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((LimitElement)element).Key;
		}

		public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;
	}
}
