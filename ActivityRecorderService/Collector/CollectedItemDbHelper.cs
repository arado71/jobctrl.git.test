using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Dapper;

namespace Tct.ActivityRecorderService.Collector
{
	public static class CollectedItemDbHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal static readonly CollectedLookupIdCache LookupIdCache = new CollectedLookupIdCache(ConfigManager.CacheSizeCollectorKeyId, ConfigManager.CacheSizeCollectorValueId);

		internal static readonly int MaxKeyLength = 4000;
		internal static readonly int MaxValueLength = 4000;

		public static void Insert(CollectedItem item, CollectedLookupIdCache cache = null)
		{
			if (cache != null)
			{
				Insert(ConvertToAggr(item), cache);
			}
			else
			{
				Insert(ConvertToAggr(item), LookupIdCache);
			}
			
		}

		public static void Insert(AggregateCollectedItems items)
		{
			Insert(items, LookupIdCache);
		}

		internal static void Insert(AggregateCollectedItems items, CollectedLookupIdCache cache)
		{
			if (items == null) return;
			if (items.Items == null || items.Items.Count == 0)
			{
				log.ErrorAndFail("AggregateCollectedItems without Items received");
				return;
			}
			Debug.Assert(items.Items.SelectMany(n => n.CapturedValues.Select(c => c.Key)).All(n => items.KeyLookup.ContainsKey(n)));
			Debug.Assert(items.Items.SelectMany(n => n.CapturedValues.Where(c => c.Value.HasValue).Select(c => c.Value.Value)).All(n => items.ValueLookup.ContainsKey(n)));

			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				conn.Open();
				List<KeyValuePair<string, int>> createdKeys = new List<KeyValuePair<string, int>>();
				List<KeyValuePair<string, int>> createdValues = new List<KeyValuePair<string, int>>();

				DataTable collItemsDT = CollectedItemDTHelper.CreateDT();
				DataTable keyIdNullDT = CollectedItemDTHelper.CreateDT();
				DataTable valueIdNullDT = CollectedItemDTHelper.CreateDT();

				
				var clientKeyIdToServerId = new Dictionary<int, int>(items.KeyLookup.Count);
				var clientValueIdToServerId = new Dictionary<int, int>(items.ValueLookup.Count);
				Dictionary<int, string> encryptedValueLookup = null;

				if (ConfigManager.IsCollectedValuesEncrypted)
				{
					encryptedValueLookup = new Dictionary<int, string>(items.ValueLookup.Count);
					using (var encrypter = new StringCipher())
					{
						foreach (var item in items.ValueLookup)
						{
							try
							{
								encryptedValueLookup.Add(item.Key, GetLongestPossibleEncryptedValue(encrypter, item.Value));
							}
							catch (ArgumentNullException ex)
							{
								encryptedValueLookup.Add(item.Key, null);
							}
						}
					}
				}

				foreach (var item in items.Items)
				{
					foreach (var capturedValue in item.CapturedValues)
					{
						int? keyId;
						string key;
						int? valueId = null;
						string value = null;

						GetKeyOrId(capturedValue.Key, clientKeyIdToServerId, items.KeyLookup, cache, out keyId, out key);
						if (capturedValue.Value != null)
						{
							GetValueOrId(capturedValue.Value.Value, clientValueIdToServerId, encryptedValueLookup ?? items.ValueLookup, cache, out valueId, out value);
						}

						if (keyId == null)
						{
							if (key == null)
							{
								log.Error("Key cannot be empty. This captured value is ingored.");
								continue;
							}
							CollectedItemDTHelper.AddRowToDT(keyIdNullDT, items.UserId, item.CreateDate, items.ComputerId, keyId, key, valueId, valueId == null ? value : null);
						}

						if (valueId == null && value != null)
						{
							CollectedItemDTHelper.AddRowToDT(valueIdNullDT, items.UserId, item.CreateDate, items.ComputerId, keyId, keyId == null ? key : null, valueId, value);
						}
						CollectedItemDTHelper.AddRowToDT(collItemsDT, items.UserId, item.CreateDate, items.ComputerId, keyId, keyId == null ? key : null, valueId, valueId == null ? value : null);

					}
				}


				using (var tran = conn.BeginTransaction())
				{
					if (keyIdNullDT.Rows.Count > 0)
					{
						SqlParameter paramKeyNull = new SqlParameter
						{
							ParameterName = "@Items",
							SqlDbType = SqlDbType.Structured,
							Value = keyIdNullDT,
							TypeName = "[dbo].[CollectedItemsDataType]"
						};

						SqlCommand cmdKey = new SqlCommand
						{
							CommandText = "GetIdForCollectedKeyLight",
							CommandType = CommandType.StoredProcedure,
							Connection = conn,
							Transaction = tran
						};
						cmdKey.Parameters.Add(paramKeyNull);
						using (SqlDataReader readerKey = cmdKey.ExecuteReader())
						{
							if (readerKey.HasRows)
							{
								while (readerKey.Read())
								{
									string value = readerKey.GetString(1);
									int id = readerKey.GetInt32(0);
									CollectedItemDTHelper.ChangeRowInDT(collItemsDT, id, value, false);
									createdKeys.Add(new KeyValuePair<string, int>(value, id));
								}
							}
						}

						//remaining keyId == null rows should be handled one by one
						foreach (DataRow nullRow in CollectedItemDTHelper.GetKeyIdNullRows(collItemsDT))
						{
							var dynParam = new DynamicParameters();
							dynParam.Add("@key", nullRow["Key"], dbType: DbType.String, direction: ParameterDirection.Input, size: 4000);
							dynParam.Add("@id", nullRow["KeyId"], dbType: DbType.Int32, direction: ParameterDirection.InputOutput, size: 4);
							conn.Execute("[dbo].[GetIdForCollectedKeyHard]", dynParam, tran, commandType: CommandType.StoredProcedure);
							CollectedItemDTHelper.ChangeRowInDT(collItemsDT, dynParam.Get<int>("@id"), (string)nullRow["Key"], false);
							createdKeys.Add(new KeyValuePair<string, int>((string)nullRow["Key"], dynParam.Get<int>("@id")));
						} 
					}

					if (valueIdNullDT.Rows.Count > 0)
					{
						SqlParameter paramValueNull = new SqlParameter
						{
							ParameterName = "@Items",
							SqlDbType = SqlDbType.Structured,
							Value = valueIdNullDT,
							TypeName = "[dbo].[CollectedItemsDataType]"
						};

						SqlCommand cmdValue = new SqlCommand
						{
							CommandText = "GetIdForCollectedValueLight",
							CommandType = CommandType.StoredProcedure,
							Connection = conn,
							Transaction = tran
						};
						cmdValue.Parameters.Add(paramValueNull);
						using (SqlDataReader readerValue = cmdValue.ExecuteReader())
						{
							if (readerValue.HasRows)
							{
								while (readerValue.Read())
								{
									string value = readerValue.IsDBNull(1) ? string.Empty : readerValue.GetString(1);
									int id = readerValue.GetInt32(0);
									CollectedItemDTHelper.ChangeRowInDT(collItemsDT, id, value, true);
									createdValues.Add(new KeyValuePair<string, int>(value, id));
								}
							}
						}

						//remaining valueId == null rows should be handled one by one
						foreach (DataRow nullRow in CollectedItemDTHelper.GetValueIdNullRows(collItemsDT))
						{
							var dynParam = new DynamicParameters();
							dynParam.Add("@value", nullRow["Value"], dbType: DbType.String, direction: ParameterDirection.Input, size: 4000);
							dynParam.Add("@id", nullRow["ValueId"], dbType: DbType.Int32, direction: ParameterDirection.InputOutput, size: 4);
							conn.Execute("[dbo].[GetIdForCollectedValueHard]", dynParam, tran, commandType: CommandType.StoredProcedure);
							CollectedItemDTHelper.ChangeRowInDT(collItemsDT, dynParam.Get<int>("@id"), (string)nullRow["Value"], true);
							createdValues.Add(new KeyValuePair<string, int>((string)nullRow["Value"], dynParam.Get<int>("@id")));
						} 
					}



					SqlParameter paramCollItems = new SqlParameter
					{
						ParameterName = "@CollectedItems",
						SqlDbType = SqlDbType.Structured,
						Value = collItemsDT,
						TypeName = "[dbo].[CollectedItemsDataType]"
					};
					SqlParameter paramReportServerAddress = new SqlParameter
					{
						ParameterName = "@ReportServerAddress",
						SqlDbType = SqlDbType.NVarChar,
						Size = 50,
						Value = ConfigManager.ReportServerAddress
					};

					SqlCommand cmdCollItems = new SqlCommand
					{
						CommandText = "InsertCollectedItems",
						CommandType = CommandType.StoredProcedure,
						Connection = conn,
						Transaction = tran
					};
					cmdCollItems.Parameters.Add(paramCollItems);
					cmdCollItems.Parameters.Add(paramReportServerAddress);
					try
					{
						cmdCollItems.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						log.Error("InsertCollectedItems failed with, ", ex);
						log.DebugFormat("Stored procedure call parameters: Rows of DataTable: {0}, Number of items: {1}",
							collItemsDT.Rows.Count, items.Items.Count);
						throw;
					}

					tran.Commit();
				}
				if (createdKeys != null)
				{
					foreach (var kvp in createdKeys)
					{
						cache.AddKey(kvp.Key, kvp.Value);
					}
				}
				if (createdValues != null)
				{
					foreach (var kvp in createdValues)
					{
						cache.AddValue(kvp.Key, kvp.Value);
					}
				}
			}
		}

		public static string GetLongestPossibleEncryptedValue(StringCipher cipher, string value)
		{
			var newValue = cipher.Encrypt(value);
			if (newValue.Length <= MaxValueLength)
				return newValue;

			// If encrypted value is longer than the limit we have to find the longest possible raw data which can be encrypted
			int minLength = 0;
			int maxLength = value.Length;
			while (minLength <= maxLength)
			{
				var length = (minLength + maxLength) / 2;
				newValue = cipher.Encrypt(value.Substring(0, length));
				if (newValue.Length > MaxValueLength)
					maxLength = length - 1;
				else
					minLength = length + 1;
			}

			return cipher.Encrypt(value.Substring(0, maxLength));
		}


		private static void GetKeyOrId(int clientKeyId, Dictionary<int, int> clientKeyIdToServerId, Dictionary<int, string> clientKeyLookup, CollectedLookupIdCache cache, out int? keyId, out string key)
		{
			int serverKeyId;
			if (clientKeyIdToServerId.TryGetValue(clientKeyId, out serverKeyId))
			{
				key = null;
				keyId = serverKeyId;
			}
			else
			{
				var rawKey = clientKeyLookup[clientKeyId];
				key = rawKey.Length > MaxKeyLength ? rawKey.Substring(0, MaxKeyLength) : rawKey;
				keyId = cache.GetIdForKey(key);
				if (keyId.HasValue) clientKeyIdToServerId.Add(clientKeyId, keyId.Value);
			}
		}

		private static void GetValueOrId(int clientValueId, Dictionary<int, int> clientValueIdToServerId, Dictionary<int, string> clientValueLookup, CollectedLookupIdCache cache, out int? valueId, out string value)
		{
			int serverValueId;
			if (clientValueIdToServerId.TryGetValue(clientValueId, out serverValueId))
			{
				value = null;
				valueId = serverValueId;
			}
			else
			{
				var rawValue = clientValueLookup[clientValueId];
				value = rawValue.Length > MaxValueLength ? rawValue.Substring(0, MaxValueLength) : rawValue;
				valueId = cache.GetIdForValue(value);
				if (valueId.HasValue) clientValueIdToServerId.Add(clientValueId, valueId.Value);
			}
		}

		private static AggregateCollectedItems ConvertToAggr(CollectedItem item)
		{
			var res = GetEmptyAggrItem(item);
			AddCollectedItem(res, item);
			return res;
		}

		private static AggregateCollectedItems GetEmptyAggrItem(CollectedItem item)
		{
			return new AggregateCollectedItems()
			{
				UserId = item.UserId,
				ComputerId = item.ComputerId,
				KeyLookup = new Dictionary<int, string>(),
				ValueLookup = new Dictionary<int, string>(),
				Items = new List<CollectedItemIdOnly>()
				{
					new CollectedItemIdOnly()
					{
						CreateDate = item.CreateDate,
						CapturedValues = new Dictionary<int, int?>(),
					}
				},
			};
		}

		private static void AddCollectedItem(AggregateCollectedItems target, CollectedItem item)
		{
			var res = new CollectedItemIdOnly()
			{
				CreateDate = item.CreateDate,
				CapturedValues = new Dictionary<int, int?>(item.CapturedValues.Count)
			};
			foreach (var capturedValue in item.CapturedValues)
			{
				var keyId = target.KeyLookup.Where(n => n.Value == capturedValue.Key).Select(n => new int?(n.Key)).FirstOrDefault();
				if (keyId == null)
				{
					keyId = target.KeyLookup.Select(n => n.Key).DefaultIfEmpty(0).Max() + 1;
					target.KeyLookup.Add(keyId.Value, capturedValue.Key);
				}

				var valueId = capturedValue.Value == null ? new int?() : target.ValueLookup.Where(n => n.Value == capturedValue.Value).Select(n => new int?(n.Key)).FirstOrDefault();
				if (valueId == null && capturedValue.Value != null)
				{
					valueId = target.ValueLookup.Select(n => n.Key).DefaultIfEmpty(0).Max() + 1;
					target.ValueLookup.Add(valueId.Value, capturedValue.Value);
				}

				res.CapturedValues.Add(keyId.Value, valueId);
			}
			target.Items.Add(res);
		}

	}
}
