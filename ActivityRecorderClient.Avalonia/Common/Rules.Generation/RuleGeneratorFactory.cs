using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	public class RuleGeneratorFactory
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly List<Func<RuleGeneratorData, IRuleGenerator>> knownGenerators = new List<Func<RuleGeneratorData, IRuleGenerator>>()
		{
			n => GetSimpleRuleGenerator(n),
			n => GetIgnoreRuleGenerator(n),
			n => GetReplaceGroupRuleGenerator(n),
		};

		public static IRuleGenerator CreateGeneratorFromData(RuleGeneratorData data)
		{
			if (data == null) return null;
			foreach (var knownGenerator in knownGenerators)
			{
				try
				{
					var ruleGen = knownGenerator(data);
					if (ruleGen != null) return ruleGen;
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unexpected error for name " + data.Name + " and params " + data.Parameters, ex);
				}
			}
			log.Warn("Unknown rule generator with name " + data.Name + " and params " + data.Parameters);
			return null;
		}

		public static RuleGeneratorData GetDataFromCreateParams(IRuleGeneratorCreateParams createParams)
		{
			if (createParams == null) return null;
			var simple = createParams as SimpleRuleGeneratorCreateParams;
			if (simple != null)
			{
				return new RuleGeneratorData()
				{
					Name = "SimpleRuleGenerator",
					Parameters = SerializeData(simple),
				};
			}
			var ignore = createParams as IgnoreRuleGeneratorCreateParams;
			if (ignore != null)
			{
				return new RuleGeneratorData()
				{
					Name = "IgnoreRuleGenerator",
					Parameters = SerializeData(ignore),
				};
			}
			var replace = createParams as ReplaceGroupRuleGeneratorCreateParams;
			if (replace != null)
			{
				return new RuleGeneratorData()
				{
					Name = "ReplaceGroupRuleGenerator",
					Parameters = SerializeData(replace),
				};
			}
			log.Warn("Unknow Type of IRuleGeneratorCreateParams " + createParams.GetType());
			return null;
		}

		private static SimpleRuleGenerator GetSimpleRuleGenerator(RuleGeneratorData data)
		{
			if (data.Name != "SimpleRuleGenerator") return null;
			var ctorData = DeserializeData<SimpleRuleGeneratorCreateParams>(data.Parameters);
			return new SimpleRuleGenerator(ctorData.IgnoreCase);
		}

		private static IgnoreRuleGenerator GetIgnoreRuleGenerator(RuleGeneratorData data)
		{
			if (data.Name != "IgnoreRuleGenerator") return null;
			var ctorData = DeserializeData<IgnoreRuleGeneratorCreateParams>(data.Parameters);
			return new IgnoreRuleGenerator(ctorData.IgnoreCase, ctorData.ProcessNamePattern, ctorData.TitlePattern, ctorData.UrlPattern);
		}

		private static ReplaceGroupRuleGenerator GetReplaceGroupRuleGenerator(RuleGeneratorData data)
		{
			if (data.Name != "ReplaceGroupRuleGenerator") return null;
			var ctorData = DeserializeData<ReplaceGroupRuleGeneratorCreateParams>(data.Parameters);
			return new ReplaceGroupRuleGenerator(ctorData.IgnoreCase, ctorData.ProcessNameParams, ctorData.TitleParams, ctorData.UrlParams);
		}

		private static T DeserializeData<T>(string data)
		{
			var bytes = Encoding.UTF8.GetBytes(data);
			var ser = new DataContractJsonSerializer(typeof(T));
			using (var stream = new MemoryStream(bytes, false))
			{
				return (T)ser.ReadObject(stream);
			}
		}

		private static string SerializeData<T>(T data)
		{
            var settings = new DataContractJsonSerializerSettings
            {
                MaxItemsInObjectGraph = int.MaxValue,
                UseSimpleDictionaryFormat = true,
                IgnoreExtensionDataObject = false
            };

            var ser = new DataContractJsonSerializer(typeof(T), settings); //extension data is lost, but that is ok atm.

            using (var stream = new MemoryStream())
			{
				ser.WriteObject(stream, data);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
	}


}
