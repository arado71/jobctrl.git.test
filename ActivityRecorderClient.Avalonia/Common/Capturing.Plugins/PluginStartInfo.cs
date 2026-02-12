using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public class PluginStartInfo : IEquatable<PluginStartInfo>
	{
		public string PluginId { get; set; }
		public List<ExtensionRuleParameter> Parameters { get; set; }
		public PluginStartInfoDetails Details { get; set; }

		public bool Equals(PluginStartInfo other)
		{
			if (other == null) return false;
			return PluginId == other.PluginId &&
				Parameters.CollectionEqual(other.Parameters);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PluginStartInfo);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (PluginId == null ? 0 : PluginId.GetHashCode());
			if (Parameters != null && Parameters.Count > 0)
			{
				foreach (var parameter in Parameters)
				{
					result = 31 * result + (parameter == null ? 0 : parameter.GetHashCode());
				}
			}
			return result;
		}

		public override string ToString()
		{
			return PluginId + (Parameters == null ? "" : (" (" + string.Join(", ", Parameters.Where(n => n != null).Select(n => n.ToString()).ToArray()) + ")"));
		}
	}
}
