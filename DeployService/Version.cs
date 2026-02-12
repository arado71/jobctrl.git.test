using System.Text.RegularExpressions;
using log4net;

namespace Tct.DeployService
{
	public class Version
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(Version));

		public int Major { get; set; }
		public int Minor { get; set; }
		public int Revision { get; set; }
		public int Build { get; set; }

		public static Version Parse(string version)
		{
			var matches = Regex.Match(version, "([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)");
			if (!matches.Success)
			{
				logger.WarnFormat("Malformed version: \"{0}\"", version);
				return null;
			}

			return new Version
			{
				Major = int.Parse(matches.Groups[1].Value),
				Minor = int.Parse(matches.Groups[2].Value),
				Revision = int.Parse(matches.Groups[3].Value),
				Build = int.Parse(matches.Groups[4].Value)
			};
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Version)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Major;
				hashCode = (hashCode * 397) ^ Minor;
				hashCode = (hashCode * 397) ^ Revision;
				hashCode = (hashCode * 397) ^ Build;
				return hashCode;
			}
		}

		public static bool operator<(Version left, Version right)
		{
			if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
			if (left.Major < right.Major) return true;
			if (left.Major > right.Major) return false;
			if (left.Minor < right.Minor) return true;
			if (left.Minor > right.Minor) return false;
			if (left.Revision < right.Revision) return true;
			if (left.Revision > right.Revision) return false;
			return left.Build < right.Build;
		}

		public static bool operator>(Version left, Version right)
		{
			if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
			if (left.Major < right.Major) return false;
			if (left.Major > right.Major) return true;
			if (left.Minor < right.Minor) return false;
			if (left.Minor > right.Minor) return true;
			if (left.Revision < right.Revision) return false;
			if (left.Revision > right.Revision) return true;
			return left.Build > right.Build;
		}

		public static bool operator <=(Version left, Version right)
		{
			return !(left > right);
		}

		public static bool operator >=(Version left, Version right)
		{
			return !(left < right);
		}

		public static bool operator ==(Version left, Version right)
		{
			if (ReferenceEquals(left, null) != ReferenceEquals(right, null)) return false;
			return ReferenceEquals(left, null) || left.Equals(right);
		}

		public static bool operator !=(Version left, Version right)
		{
			return !(left == right);
		}


		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
		}

		protected bool Equals(Version other)
		{
			return Major == other.Major && Minor == other.Minor && Revision == other.Revision && Build == other.Build;
		}
	}
}
