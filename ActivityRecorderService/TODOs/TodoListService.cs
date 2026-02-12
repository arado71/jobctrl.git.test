using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.Messaging;

namespace Tct.ActivityRecorderService.TODOs
{
	class TodoListService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Dictionary<byte, TodoListItemStatusDTO> statusDictionary;

		public static readonly TodoListService Instance;
		private readonly object lockObject = new object();
#if DEBUG
		private readonly Dictionary<int, Dictionary<DateTime, TodoListDTO>> userTodoListDictionary = new Dictionary<int, Dictionary<DateTime, TodoListDTO>>();
#endif

		static TodoListService()
		{
			Instance = new TodoListService();
		}

		protected TodoListService()
		{
			refreshTodoListStatuses();
		}

		private void refreshTodoListStatuses()
		{
#if DEBUG
			statusDictionary = new List<TodoListItemStatusDTO>
			{
				new TodoListItemStatusDTO { Id = 1, Name = "Opened" },
				new TodoListItemStatusDTO { Id = 2, Name = "Finished" },
				new TodoListItemStatusDTO { Id = 3, Name = "Postponed" },
				new TodoListItemStatusDTO { Id = 4, Name = "Canceled" }
			}.ToDictionary(x => x.Id);
			return;
#endif
			Dictionary<byte, TodoListItemStatusDTO> newDictionary = new Dictionary<byte, TodoListItemStatusDTO>();
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				foreach (var status in context.TodoListItemStatus.OrderBy(x => x.Id))
				{
					newDictionary.Add(status.Id, new TodoListItemStatusDTO { Id = status.Id, Name = status.Name });
				}
			}
			lock (lockObject)
			{
				statusDictionary = newDictionary;
			}
		}

		public TodoListDTO GetLastTodoList(int userId)
		{
			Stopwatch sw = Stopwatch.StartNew();
#if DEBUG
			var temp = getDebugTodoList(userId, DateTime.Today);
			log.Debug($"GetLastTodoList finished in {sw.ToTotalMillisecondsString()} ms for user ({userId}).");
			return temp;
#endif
			try
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var queryResult = context.GetLastTodoListOfUser(userId, userId)?.OrderBy(x => x.Priority).ToList();
					if (queryResult == null || !queryResult.Any()) return null;
					List<TodoListItemDTO> todoListItemDtos = new List<TodoListItemDTO>();
					DateTime day = DateTime.MinValue;
					int listId = -1;
					DateTime? lockLastTakenAt = null;
					DateTime listCreatedAt = DateTime.MinValue;
					foreach (var row in queryResult)
					{
						if (listId == -1)
						{
							listId = row.Id;
							day = row.Day;
							listCreatedAt = row.CreatedAt;
							lockLastTakenAt = row.LockLastTakenAt;
						}

						if (row.Status == null || row.Priority == null || row.ListItemId == null)
							break;
						TodoListItemStatusDTO status;
						if (!statusDictionary.TryGetValue((byte)row.Status, out status))
						{
							refreshTodoListStatuses();
							status = statusDictionary[(byte)row.Status];
						}
						todoListItemDtos.Add(new TodoListItemDTO
						{
							Id = (int)row.ListItemId,
							ListId = listId,
							Name = row.Name,
							Priority = (int)row.Priority,
							Status = status,
							CreatedAt = row.ItemCreatedAt
						});
					}
					log.Debug($"GetLastTodoList finished in {sw.ToTotalMillisecondsString()} ms for user ({userId}).");
					return new TodoListDTO
					{
						Id = listId,
						Date = day,
						TodoListItems = todoListItemDtos,
						UserId = userId,
						LockLastTakenAt = lockLastTakenAt,
						CreatedAt = listCreatedAt
					};
				}
			}
			catch (Exception e)
			{
				log.Error("Unexpected error in getting the last messages of user.", e);
				return null;
			}
		}

		public TodoListDTO GetTodoList(int userId, DateTime date)
		{
			Stopwatch sw = Stopwatch.StartNew();
#if DEBUG
			var temp = getDebugTodoList(userId, date);
			log.Debug($"GetTodoList finished in {sw.ToTotalMillisecondsString()} ms for user ({userId}), date ({date.ToShortDateString()}).");
			return temp;
#endif
			try
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var queryResult = context.GetTodoListsOfUser(userId, userId, date)?.OrderBy(x => x.Priority).ToList();
					List<TodoListItemDTO> todoListItemDtos = new List<TodoListItemDTO>();
					DateTime day = DateTime.MinValue;
					int listId = -1;
					DateTime? lockLastTakenAt = null;
					DateTime listCreatedAt = DateTime.MinValue;
					if (queryResult == null || !queryResult.Any()) return null;
					foreach (var row in queryResult)
					{
						if (listId == -1)
						{
							listId = row.Id;
							day = row.Day;
							listCreatedAt = row.CreatedAt;
							lockLastTakenAt = row.LockLastTakenAt;
						}

						if (row.Status == null || row.Priority == null || row.ListItemId == null)
							break;
						TodoListItemStatusDTO status;
						if (!statusDictionary.TryGetValue((byte)row.Status, out status))
						{
							refreshTodoListStatuses();
							status = statusDictionary[(byte)row.Status];
						}

						todoListItemDtos.Add(new TodoListItemDTO
						{
							Id = (int)row.ListItemId,
							ListId = listId,
							Name = row.Name,
							Priority = (int)row.Priority,
							Status = status,
							CreatedAt = row.ItemCreatedAt
						});
					}
					log.Debug($"GetTodoList finished in {sw.ToTotalMillisecondsString()} ms for user ({userId}), date ({date.ToShortDateString()}).");
					Debug.Assert(listCreatedAt != DateTime.MinValue, "listCreatedAt != DateTime.MinValue");
					return new TodoListDTO
					{
						Id = listId,
						Date = day,
						TodoListItems = todoListItemDtos,
						UserId = userId,
						LockLastTakenAt = lockLastTakenAt,
						CreatedAt = listCreatedAt
					};
				}
			}
			catch (Exception e)
			{
				log.Error("Unexpected exception in GetTodoList.", e);
				return null;
			}
		}


		public bool CreateOrUpdateTodoList(TodoListDTO todoList)
		{
			Stopwatch sw = Stopwatch.StartNew();
#if DEBUG
			try
			{
				if (!userTodoListDictionary.ContainsKey(todoList.UserId))
				{
					userTodoListDictionary[todoList.UserId] = new Dictionary<DateTime, TodoListDTO>();
				}

				userTodoListDictionary[todoList.UserId][todoList.Date] = todoList;
				var userId = todoList.UserId;
				UserStatInfo userStatInfo = StatsDbHelper.GetUserStatsInfo(new List<int>(new int[] { userId })).First();
				var culture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(userStatInfo.CultureId)
					? EmailStatsHelper.DefaultCulture
					: userStatInfo.CultureId);
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;
				var content = string.Format(EmailStats.EmailStats.TODOs_NotificationMessage, userStatInfo.FirstName,
					userStatInfo.LastName, todoList.Date.ToShortDateString(), todoList.UserId,
					todoList.Date.ToString("yyyy-MM-dd"));
				MessageService.Instance.InsertMessage(userId, userId, content, "TodoList");

				log.Debug(
					$"CreateOrUpdateTodoList finished in {sw.ToTotalMillisecondsString()} ms for user ({todoList.UserId}), date ({todoList.Date.ToShortDateString()}).");
				return true;
			}
			catch (Exception e)
			{
				log.Error("Exception in CreateTodoList.", e);
				return false;
			}
#endif
			try
			{
				bool modified = false;
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.Connection.Open();
					using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
					{
						int res;
						var old = context.GetTodoListsOfUser(todoList.UserId, todoList.UserId, todoList.Date).ToList();
						if (!old.Any())
						{
							int? id = -1;
							modified = true;
							res = context.CreateTodoList(todoList.UserId, todoList.UserId, todoList.Date, ref id);
							if (res != 0) throw new Exception($"Database stored procedure error in CreateTodoList. Code: {res}");
							foreach (var item in todoList.TodoListItems)
							{
								int? itemId = -1;
								res = context.CreateTodoListItem(todoList.UserId, id, item.Name, item.Priority, item.CreatedAt, ref itemId);
								if (res != 0) throw new Exception($"Database stored procedure error in CreateTodoListItem. Code: {res}");
								if (item.Status.Id != 1)
								{
									res = context.SetStatusForTodoListItem(todoList.UserId, itemId, item.Status.Id);
									if (res != 0) throw new Exception($"Database stored procedure error in SetStatusForTodoListItem. Code: {res}");
								}
							}
						}
						else
						{
							if (todoList.Id == default(int)) throw new FaultException("List already exists.");
							foreach (var item in todoList.TodoListItems)
							{
								GetTodoListsOfUserResult oldItem = null;
								foreach (var result in old)
								{
									todoList.Id = result.Id;
									if (result.ListItemId == item.Id)
									{
										oldItem = result;
										break;
									}
								}

								if (oldItem != null)
								{
									if (!oldItem.Name.Equals(item.Name, StringComparison.InvariantCulture) || oldItem.Priority != item.Priority)
									{
										modified = true;
										res = context.UpdateTodoListItem(todoList.UserId, item.Id, item.Name, item.Priority);
										if (res != 0) throw new Exception($"Database stored procedure error in UpdateTodoListItem. Code: {res}");
									}

									if (oldItem.Status != item.Status.Id)
									{
										modified = true;
										res = context.SetStatusForTodoListItem(todoList.UserId, item.Id, item.Status.Id);
										if (res != 0) throw new Exception($"Database stored procedure error in SetStatusForTodoListItem. Code: {res}");
									}
								}
								else
								{
									int? id = null;
									modified = true;
									res = context.CreateTodoListItem(todoList.UserId, todoList.Id, item.Name, item.Priority, item.CreatedAt, ref id);
									if (res != 0) throw new Exception($"Database stored procedure error in CreateTodoListItem. Code: {res}");
									Debug.Assert(id != null, nameof(id) + " != null");
									item.Id = (int) id;
									if (item.Status.Id != 1)
									{
										res = context.SetStatusForTodoListItem(todoList.UserId, item.Id, item.Status.Id);
										if (res != 0) throw new Exception($"Database stored procedure error in SetStatusForTodoListItem. Code: {res}");
									}
								}
							}
						}
						context.Transaction.Commit();
					}
				}

				if (modified)
				{
					ThreadPool.QueueUserWorkItem(x =>
					{
						try
						{
							using (var context = new ActivityRecorderDataClassesDataContext())
							{
								var userId = todoList.UserId;
								UserStatInfo userStatInfo = StatsDbHelper.GetUserStatsInfo(new List<int>(new int[] { userId })).First();
								var firstName = userStatInfo.FirstName;
								var lastName = userStatInfo.LastName;
								if (!UserIdManager.Instance.TryGetIdsForUser(userId, out _, out int companyId))
								{
									log.Error($"Couldn't get companyId for user ({userId})");
									throw new FaultException("User is not active");
								}

								var queryResult = context.Client_GetSupervisorsOfWorker(companyId, userId, ConfigManager.TodoNotificationToAdminsEnabled);
								CultureInfo culture = null;
								foreach (var clientGetSupervisorsOfWorkerResult in queryResult)
								{
									if (clientGetSupervisorsOfWorkerResult.UserId is int superVisorUserId && superVisorUserId != userId)
									{
										try
										{
											log.DebugFormat("Inserting message for user {0}", superVisorUserId);
											var tempCulture = CultureInfo.GetCultureInfo(
												string.IsNullOrEmpty(clientGetSupervisorsOfWorkerResult.CultureId)
													? EmailStatsHelper.DefaultCulture
													: clientGetSupervisorsOfWorkerResult.CultureId);
											if (!tempCulture.Equals(culture))
											{
												culture = tempCulture;
												Thread.CurrentThread.CurrentCulture = culture;
												Thread.CurrentThread.CurrentUICulture = culture;
											}

											var content = string.Format(EmailStats.EmailStats.TODOs_NotificationMessage, firstName,
												lastName, todoList.Date.ToShortDateString(), todoList.UserId,
												todoList.Date.ToString("yyyy-MM-dd"));
											MessageService.Instance.InsertMessage(userId, superVisorUserId, content, "TodoList");
										}
										catch (Exception e)
										{
											log.Error(
												$"Inserting message for supervisor failed. supervisorUserId: {superVisorUserId}, employeeUserId: {userId} date: {todoList.Date}",
												e);
										}
									}
								}
							}
						}
						catch (FaultException)
						{
							throw;
						}
						catch (Exception e)
						{
							log.Error(
								$"unexpected exception in CreateOrUpdateTodoList: Todolist id:{todoList.Id}, UserId: {todoList.UserId} date: {todoList.Date}",
								e);
						}
					});
				}

				log.Debug($"CreateOrUpdateTodoList finished in {sw.ToTotalMillisecondsString()} ms for user ({todoList.UserId}), date ({todoList.Date.ToShortDateString()}).");
				return true;

			}
			catch (FaultException) { throw; }
			catch (Exception e)
			{
				log.Error($"Creating or updating todolist failed. userId: {todoList.UserId}, date: {todoList.Date}", e);
				return false;
			}
		}

		public List<TodoListItemStatusDTO> GetTodoListItemStatuses()
		{
			refreshTodoListStatuses();
			return statusDictionary.Values.ToList();
		}

#if DEBUG
		private int counter = 0;
#endif
		public TodoListToken AcquireTodoListLock(int userId, int todoListId)
		{
#if DEBUG
			if (counter++ < 1)
				return new TodoListToken(false, "Kala", "Pál");
			return new TodoListToken(true, "Kala", "Pál");
#endif
			try
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var queryResult = context.TryAcquireTodoListLock(userId, todoListId);
					if (queryResult.ReturnValue is int queryResultAsInt)
					{
						if (queryResultAsInt == 0)
							return new TodoListToken(true);
						if (queryResultAsInt == 106)
							return new TodoListToken(false);
						if (queryResultAsInt == 1001)
						{
							var queryResultRow = queryResult.First();
							var lastName = queryResultRow.LastName;
							var firstName = queryResultRow.FirstName;
							return new TodoListToken(false, lastName, firstName);
						}
						log.Error("TryAcquireTodoListLock query result wasn't in the expected range {0, 106, 1001}.");
						return new TodoListToken(false);
					}
					log.Error("TryAcquireTodoListLock query result wasn't int.");
					return new TodoListToken(false);
				}
			}
			catch (Exception e)
			{
				log.Error(
					$"An unexpected error occured in getting lock for the todolist. userid: {userId}, todolistId: {todoListId}", e);
				return new TodoListToken(false);
			}
		}

		public bool ReleaseTodoListLock(int userId, int todoListId)
		{
#if DEBUG
			return true;
#endif
			try
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var queryResult = context.ReleaseTodoListLock(userId, todoListId);
					if (queryResult is int queryResultAsInt)
					{
						switch (queryResultAsInt)
						{
							case 0:
								return true;
							case 106:
								log.ErrorFormat("Acces denied in ReleaseTodoListLock! UserId: {0}, TodoListId: {1}", userId, todoListId);
								break;
							case 1001:
								log.DebugFormat("Lock already released in ReleaseTodoListLock! UserId: {0}, TodoListId: {1}", userId, todoListId);
								return true;
							case 1002:
								log.ErrorFormat("Lock not owned in ReleaseTodoListLock. UserId: {0}, TodoListId: {1}", userId, todoListId);
								break;
						}
					}
				}
				return false;
			}
			catch (Exception e)
			{
				log.Error(
					$"An unexpected error occured in getting lock for the todolist. userid: {userId}, todolistId: {todoListId}", e);
				return false;
			}
		}



#if DEBUG
		private TodoListDTO getDebugTodoList(int userId, DateTime date)
		{
			if (userTodoListDictionary.ContainsKey(userId) && userTodoListDictionary[userId].ContainsKey(date))
				return userTodoListDictionary[userId][date];
			if (date > DateTime.Today.AddDays(-1)) return null;
			return new TodoListDTO()
			{
				Id = 0,
				Date = date,
				TodoListItems = new List<TodoListItemDTO> {
					new TodoListItemDTO
					{
						Id = 1,
						ListId = 0,
						Name = "Some task",
						Priority = 0,
						Status = new TodoListItemStatusDTO
						{
							Id = 0,
							Name = "Canceled"
						}
					},
					new TodoListItemDTO
					{
						Id = 2,
						ListId = 0,
						Name = "Some other task",
						Priority = 1,
						Status = new TodoListItemStatusDTO
						{
							Id = 1,
							Name = "Opened"
						},
						CreatedAt = DateTime.Today.AddDays(-25)
					}},
				UserId = userId
			};
		}
#endif
	}
}
