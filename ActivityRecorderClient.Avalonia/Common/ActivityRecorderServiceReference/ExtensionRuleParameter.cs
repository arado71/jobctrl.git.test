using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class ExtensionRuleParameter : IEquatable<ExtensionRuleParameter>
	{
		public bool Equals(ExtensionRuleParameter other)
		{
			if (other == null) return false;
			return Name == other.Name && Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ExtensionRuleParameter);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (Name == null ? 0 : Name.GetHashCode());
			result = 31 * result + (Value == null ? 0 : Value.GetHashCode());
			return result;
		}

		public override string ToString()
		{
			return Name + "=" + Value;
		}
	}
}
