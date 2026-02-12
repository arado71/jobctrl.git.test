using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	[DataContract]
	public class SimpleRuleGeneratorCreateParams : IRuleGeneratorCreateParams
	{
		[DataMember]
		public bool IgnoreCase { get; set; }
	}

	[DataContract]
	public class IgnoreRuleGeneratorCreateParams : IRuleGeneratorCreateParams
	{
		[DataMember]
		public bool IgnoreCase { get; set; }
		[DataMember]
		public IgnoreRuleMatchParameter ProcessNamePattern { get; set; }
		[DataMember]
		public IgnoreRuleMatchParameter TitlePattern { get; set; }
		[DataMember]
		public IgnoreRuleMatchParameter UrlPattern { get; set; }
	}

	[DataContract]
	public class ReplaceGroupRuleGeneratorCreateParams : IRuleGeneratorCreateParams
	{
		[DataMember]
		public bool IgnoreCase { get; set; }
		[DataMember]
		public ReplaceGroupParameter[] ProcessNameParams { get; set; }
		[DataMember]
		public ReplaceGroupParameter[] TitleParams { get; set; }
		[DataMember]
		public ReplaceGroupParameter[] UrlParams { get; set; }
	}

	[DataContract]
	public class ReplaceGroupParameter : ICloneable
	{
		[DataMember]
		public string MatchingPattern { get; set; }
		[DataMember]
		public string ReplaceGroupName { get; set; }

		public ReplaceGroupParameter Clone()
		{
			return (ReplaceGroupParameter)MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}

	[DataContract]
	public class IgnoreRuleMatchParameter
	{
		[DataMember]
		public string MatchingPattern { get; set; }
		[DataMember]
		public bool NegateMatch { get; set; }
	}

	public interface IRuleGeneratorCreateParams //marker interface
	{
	}
}
