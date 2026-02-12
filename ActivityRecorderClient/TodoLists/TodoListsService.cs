using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.TodoLists
{
	public class TodoListsService
	{
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private List<TodoListItemStatusDTO> statuses;
		private const int TokenQueryInterval = 10000;

		private readonly SynchronizationContext context;
		private readonly CurrentWorkController currentWorkController;
		private DateTime? lastSaveTime;
		private ReaderWriterLockSlim saveLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		public DateTime Today
		{
			get { return DateTime.Today; }
		}

		public const string ListAlreadySavedMessage = "List already exists.";

		internal TodoListDTO MostRecentTodoList { get; private set; }
		private DateTime? mostRecentTodoListLastQueried = null;

		internal IEnumerable<TodoListViewObject> MostRecentTodoListItemViews
		{
			get
			{
				return MostRecentTodoList?.TodoListItems.Select(x =>
					new TodoListViewObject(x.Id, ConvertStatus(x.Status), x.Name, x.Priority, x.CreatedAt));
			}
		}

		public DateTime? LastRecentDate
		{
			get { return MostRecentTodoList?.Date; }
		}

		private TodoListForm todoListForm;
		private bool isTodaySaved = true;
		private Tuple<int, TodoListToken> todoListToken = null;
		private readonly object tokenLock = new object();
		private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

		/// <summary>
		/// Acquire a token to modify a todolist. Shouldn't be called from the GUI thread!
		/// </summary>
		/// <param name="date"></param>
		/// <param name="todoListId"></param>
		/// <returns></returns>
		public TodoListToken AcquireTodoListToken(DateTime date, int todoListId)
		{
			log.Debug("Acquiring TodoList token...");
			lock (tokenLock)
			{
				if (todoListForm == null || !todoListForm.Visible) return null;
				try
				{
					var token = ActivityRecorderClientWrapper.Execute(x => x.AcquireTodoListLock(ConfigManager.UserId, todoListId));
					if (token.IsAcquired)
					{
						todoListToken = new Tuple<int, TodoListToken>(todoListId, token);
						return token;
					}

					log.Debug("Someone's holding the todolist token. UserId: {0}, Name: {1}");
					todoListForm.RefreshTokenholdersName(token);
				}
				catch (Exception e)
				{
					log.Debug("Couldn't acquire todolist token.", e);
				}
			}

			if (manualResetEvent.WaitOne(TokenQueryInterval))
			{
				return null;
			}

			return AcquireTodoListToken(date, todoListId);
		}

		/// <summary>
		/// Release the token after modified a todolist. Shouldn't be called from the GUI thread!
		/// </summary>
		public void ReleaseTodoListToken()
		{
			log.Debug("Releasing TodoList token...");
			lock (tokenLock)
			{
				manualResetEvent.Set();
				manualResetEvent.Reset();
				if (todoListToken == null) return;
				try
				{
					ActivityRecorderClientWrapper.Execute(x => x.ReleaseTodoListLock(ConfigManager.UserId, todoListToken.Item1));
					todoListToken = null;
				}
				catch (Exception e)
				{
					log.Debug("Couldn't release token.", e);
					Thread.Sleep(TokenQueryInterval);
					ReleaseTodoListToken();
				}
			}
		}

		public TodoListsService(SynchronizationContext guiSynchronizationContext, CurrentWorkController controller)
		{
			currentWorkController = controller;
			currentWorkController.PropertyChanged += currentWorkChanged;
			context = guiSynchronizationContext;
			context.Post(_ =>
			{
				((Platform.PlatformWinFactory)Platform.Factory).MainForm.AddEtcExtraMenuitem(() => Labels.TODOs, ShowTodoList);
				checkAndShowTodoList();
			}, null);
		}

		/// <summary>
		/// Getting last recent todolist. The caller has to take care of the exceptions.
		/// </summary>
		private void GetMostRecentTodoList()
		{
			log.Debug("Getting LastRecentTodoList.");
			var time = DateTime.UtcNow;
			var result = ActivityRecorderClientWrapper.Execute(x => x.GetMostRecentTodoList(ConfigManager.UserId));
			saveLockSlim.EnterReadLock();
			try
			{
				if(time < lastSaveTime) throw new CallSequenceInconsistencyException("Got Todolist during save!");
				MostRecentTodoList = result;
			}
			finally
			{
				saveLockSlim.ExitReadLock();
			}
			mostRecentTodoListLastQueried = Today;
		}

		internal void ShowTodoList()
		{
			if (!ConfigManager.IsTodoListEnabled) return;
			if (todoListForm != null && todoListForm.InvokeRequired)
			{
				todoListForm.Invoke(new Action(ShowTodoList));
				return;
			}
			if (todoListForm != null && todoListForm.Visible)
			{
				todoListForm.BringFront();
				return;
			}

			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					GetMostRecentTodoList();
					context.Post(__ =>
					{

						if (Today != LastRecentDate)
						{
							showTodoListAtTheStartOfTheDay();
							return;
						}

						isTodaySaved = true;
						if (todoListForm == null)
						{
							todoListForm = new TodoListForm(this);
						}

						todoListForm.Show();
					}, null);
				}
				catch (Exception e)
				{
					log.Debug("Couldn't show todoList.", e);
					context.Post(__ => { MessageBox.Show(Labels.Worktime_NoResponse, Labels.Error); }, null);
				}
			});
		}

		/// <summary>
		/// Don't call from the GUI thread!
		/// </summary>
		/// <param name="list"></param>
		/// <param name="date"></param>
		/// <returns></returns>
		public GeneralResult<bool> SaveTodoLists(IList<TodoListViewObject> list, DateTime date)
		{
			Stopwatch sw = Stopwatch.StartNew();
			try
			{
				saveLockSlim.EnterWriteLock();
				try
				{
					lastSaveTime = DateTime.UtcNow;
					foreach (var todoListViewObject in list.Reverse())
					{
						if (string.IsNullOrEmpty(todoListViewObject.Content))
						{
							list.Remove(todoListViewObject);
						}
						else break;
					}

					if (statuses == null) statuses = ActivityRecorderClientWrapper.Execute(n => n.GetTodoListItemStatuses());
					TodoListDTO todoList = new TodoListDTO
					{
						Date = date,
						UserId = ConfigManager.UserId,
						TodoListItems = list.Select(x => new TodoListItemDTO
						{
							CreatedAt = x.CreatedAt,
							Id = x.Id,
							Name = x.Content,
							Priority = x.Priority,
							Status = statuses.First(y => y.Name.Equals(x.State.ToString()))
						}).ToList()
					};
					if (MostRecentTodoList?.Date == date) todoList.Id = MostRecentTodoList.Id;
					ActivityRecorderClientWrapper.Execute(n => n.CreateOrUpdateTodoList(todoList));
					lastSaveTime = DateTime.UtcNow;
				}
				finally
				{
					saveLockSlim.ExitWriteLock();
				}
				GetMostRecentTodoList();
				isTodaySaved = true;
				return new GeneralResult<bool> { Result = true };
			}
			catch (FaultException fex)
			{
				if(fex.Message == ListAlreadySavedMessage)
				log.Warn("Couldn't save todolist because list already exists.");
				return new GeneralResult<bool> { Exception = fex };
			}
			catch (Exception ex)
			{
				log.Error("Unable to save todo list.", ex);
				return new GeneralResult<bool> { Exception = ex };
			}
			finally
			{
				log.Debug(
					$"SaveTodoLists finished in {sw.Elapsed.TotalMilliseconds.ToString("0.000", CultureInfo.InvariantCulture)} ms, date ({date.ToShortDateString()}).");
			}
		}

		internal TodoListItemState ConvertStatus(TodoListItemStatusDTO dtoStatus)
		{
			if (!Enum.TryParse(dtoStatus.Name, true, out TodoListItemState state)) state = TodoListItemState.Unspecified;
			return state;
		}

		/// <summary>
		/// Don't call from the GUI thread!
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		internal GeneralResult<IEnumerable<TodoListViewObject>> GetList(DateTime date)
		{
			Stopwatch sw = Stopwatch.StartNew();
			try
			{
				if (Today == date)
				{
					try
					{
						GetMostRecentTodoList();
					}
					catch (Exception e)
					{
						log.Error("Unable to get MostRecentTodoList.", e);
						return new GeneralResult<IEnumerable<TodoListViewObject>> { Exception = e };
					}

					if (LastRecentDate == Today)
					{
						return new GeneralResult<IEnumerable<TodoListViewObject>>
						{
							Result = MostRecentTodoList.TodoListItems.Select(x =>
								new TodoListViewObject(x.Id, ConvertStatus(x.Status), x.Name, x.Priority, x.CreatedAt))
						};
					}
					else
					{
						return new GeneralResult<IEnumerable<TodoListViewObject>>
						{
							Result = MostRecentTodoList?.TodoListItems
								.Where(x => x.Status.Name == "Postponed" || x.Status.Name == "Opened").Select(x =>
									new TodoListViewObject(x.Id, ConvertStatus(x.Status), x.Name, x.Priority, x.CreatedAt))
						};
					}
				}

				try
				{
					var res = ActivityRecorderClientWrapper.Execute(n => n.GetTodoList(ConfigManager.UserId, date));
					return new GeneralResult<IEnumerable<TodoListViewObject>>
					{
						Result = res?.TodoListItems.Select(x => new TodoListViewObject(x.Id, ConvertStatus(x.Status), x.Name, x.Priority, x.CreatedAt))
					};
				}
				catch (Exception e)
				{
					log.Error($"Unable to get todo list. Date: {date.ToShortDateString()}", e);
					return new GeneralResult<IEnumerable<TodoListViewObject>> { Exception = e };
				}
			}
			finally
			{
				log.Debug(
					$"GetList finished in {sw.Elapsed.TotalMilliseconds.ToString("0.000", CultureInfo.InvariantCulture)} ms, date ({date.ToShortDateString()}).");
			}
		}

		private void currentWorkChanged(object sender, PropertyChangedEventArgs pcea)
		{
			if (pcea.PropertyName == "CurrentWorkState")
				checkAndShowTodoList();
		}

		private void checkAndShowTodoList()
		{
			if (currentWorkController.IsWorking)
			{
				ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						if (mostRecentTodoListLastQueried != Today)
						{
							GetMostRecentTodoList();
						}
						if (Today != LastRecentDate)
							context.Post(__ => showTodoListAtTheStartOfTheDay(), null);
					}
					catch (Exception e)
					{
						log.Debug("Couldn't get MostRecentTodoList", e);
					}
				});
			}
		}

		private void showTodoListAtTheStartOfTheDay()
		{
			if (todoListForm == null)
			{
				isTodaySaved = false;
				todoListForm = new TodoListForm(this);
			}
			todoListForm.ShowForMandatorySave(MostRecentTodoList?.TodoListItems
								.Where(x => x.Status.Name == "Postponed" || x.Status.Name == "Opened").Select(x =>
									new TodoListViewObject(x.Id, ConvertStatus(x.Status), x.Name, x.Priority, x.CreatedAt)));
		}

		public void BringTodoListToTop()
		{
			if (Today != mostRecentTodoListLastQueried)
			{
				ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						GetMostRecentTodoList();

						if (LastRecentDate != Today)
							isTodaySaved = false;

						if (todoListForm != null && currentWorkController.IsWorking && !isTodaySaved)
						{
							context.Post(__ => todoListForm.ShowForMandatorySave(MostRecentTodoList?.TodoListItems.Select(x =>
								new TodoListViewObject(x.Id, ConvertStatus(x.Status), x.Name, x.Priority, x.CreatedAt))), null);
						}
					}
					catch (Exception e)
					{
						log.Debug("Couldn't show the form.", e);
					}
				});
			}
		}
	}
}