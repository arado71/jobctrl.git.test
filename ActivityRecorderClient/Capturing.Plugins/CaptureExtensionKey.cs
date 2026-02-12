using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public class CaptureExtensionKey : IEquatable<CaptureExtensionKey>
	{
		public string Id { get; private set; }
		public string Key { get; private set; }

		public CaptureExtensionKey(string id, string key)
		{
			Id = id;
			Key = key;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as CaptureExtensionKey);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (Id ?? "").GetHashCode();
			result = 31 * result + (Key ?? "").GetHashCode();
			return result;
		}

		public bool Equals(CaptureExtensionKey other)
		{
			if (other == null) return false;
			return Id == other.Id
				&& Key == other.Key;
		}

		public override string ToString()
		{
			return "CaptureKey " + Id + " " + Key;
		}
	}
}
