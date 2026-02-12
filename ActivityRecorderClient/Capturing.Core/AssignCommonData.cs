using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	[Serializable]
	public class AssignCommonData : IEquatable<AssignCommonData>
	{
		// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
		public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();

		public AssignCommonData(Dictionary<string, string> data)
		{
			data?.ToList().ForEach(kv => Data.Add(kv.Key, kv.Value));
		}

		public bool Equals(AssignCommonData other)
		{
			if (other == null) return false;
			return Data.Count == other.Data.Count && !Data.Except(other.Data).Any(); // https://stackoverflow.com/a/3804852/2295648
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((AssignCommonData) obj);
		}

		public override int GetHashCode()
		{
			return Data.GetHashCode();
		}

		public override string ToString()
		{
			return string.Join(", ", Data.Select(d => d.Key + ":" + d.Value));
		}
	}
}
