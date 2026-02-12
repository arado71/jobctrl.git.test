using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Tct.ActivityRecorderService
{
	partial class ActivityRecorderDataClassesDataContext
	{
		public string GetPluginParameter(int ruleId)
		{
#if DEBUG
			return "ocrSubj={\"AreaH\":28,\"AreaW\":518,\"AreaX\":161,\"AreaY\":92,\"Brightness\":1.0299588441848755,\"CharSet\":2,\"Contrast\":0.99940139055252075,\"HAlign\":1," +
			       "\"Interpolation\":3,\"Language\":\"eng\",\"ProcessNameRegex\":\"JobCTRL.*\\\\.exe\",\"Scale\":3.96166779471639,\"SizeH\":0,\"SizeW\":0,\"Status\":2," +
			       "\"TitleRegex\":\"TODOs\",\"TresholdChannel\":3,\"TresholdLimit\":179,\"VAlign\":1};";
#endif
			var conn = (SqlConnection)Connection;
			string ret;
			using (var cmd = new SqlCommand("SELECT ParameterValue FROM PluginParametersOfAutoRules WHERE AutoRuleId=@RuleId", conn))
			{
				conn.Open();
				cmd.Parameters.AddWithValue("@RuleId", ruleId);
				ret = (string)cmd.ExecuteScalar();
			}
			conn.Close();
			return ret;
		}
		public Versioned<ClientMenu> GetClientMenu(int userId)
		{
			return GetValueFromDb<ClientMenu>(userId, "SELECT [Menu], [MenuVersion] FROM [dbo].[ClientSettings] WHERE [UserId] = @userId");
		}

		public Versioned<List<WorkDetectorRule>> GetWorkDetectorRules(int userId)
		{
			return GetValueFromDb<List<WorkDetectorRule>>(userId, "SELECT [WorkDetectorRules], [WorkDetectorRulesVersion] FROM [dbo].[ClientSettings] WHERE [UserId] = @userId");
		}

		public Versioned<List<CensorRule>> GetCensorRules(int userId)
		{
			return GetValueFromDb<List<CensorRule>>(userId, "SELECT [CensorRules], [CensorRulesVersion] FROM [dbo].[ClientSettings] WHERE [UserId] = @userId");
		}

		public Versioned<CollectorRules> GetCollectorRules(int userId)
		{
			return GetValueFromDb<CollectorRules>(userId, "SELECT [CollectorRules], [CollectorRulesVersion] FROM [dbo].[ClientSettings] WHERE [UserId] = @userId");
		}

		private Versioned<T> GetValueFromDb<T>(int userId, string sqlText) where T : class
		{
			var conn = (SqlConnection)Connection;
			using (var cmd = new SqlCommand(sqlText, conn))
			{
				cmd.Parameters.AddWithValue("userId", userId);
				conn.Open();
				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					Versioned<T> result = null;
					while (reader.Read())
					{
						if (result != null) throw new InvalidOperationException();
						result = new Versioned<T>();
						if (reader.IsDBNull(0))
						{
							result.Value = default(T);
						}
						else
						{
							using (var data = XmlReader.Create(reader.GetTextReader(0), GetSettings()))
							{
								DataContractSerializer serializer = new DataContractSerializer(typeof(T));
								result.Value = (T)serializer.ReadObject(data);
							}
						}

						var bin = reader.GetSqlBinary(1);
						result.Version = GetVersionString(bin);
					}
					return result;
				}
			}
		}

		private static string GetVersionString(SqlBinary bin)
		{
			if (bin.IsNull) return null;
			var sb = new StringBuilder();
			sb.Append("\"");
			sb.Append(Convert.ToBase64String(bin.Value, 0, bin.Value.Length));
			sb.Append("\"");
			return sb.ToString();
		}

		private static XmlReaderSettings GetSettings()
		{
			return new XmlReaderSettings()
			{
				// Since we will immediately wrap the TextReader we are creating in an XmlReader, we will permit the XmlReader to take care of closing\disposing it
				CloseInput = true,
			};
		}

		public DateTime? GetPasswordExpiry(int userId)
		{
#if D2EBUG
			return userId == 25 ? DateTime.Today.AddDays(5) : (DateTime?)null;
#endif
			return ExecuteQuery<DateTime?>(
				"SELECT [PasswordExpiresAt] FROM [dbo].[ClientSettings] WHERE UserId={0}", userId).SingleOrDefault();
		}
	}

	public class Versioned<T> where T : class
	{
		public T Value { get; set; }
		public string Version { get; set; }
	}
}
