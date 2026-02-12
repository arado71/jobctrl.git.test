using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Controller;

namespace Tct.ActivityRecorderClient.TodoLists
{
	class TodoListsManager : PeriodicManager
	{
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#if DEBUG || DEV
		private const int callbackInterval = 1 * 30 * 1000;
#else
		private const int callbackInterval = 1 * 60 * 1000;
#endif
		private TodoListsService todoListsService;

		private readonly SynchronizationContext guiSynchronizationContext;
		private readonly CurrentWorkController currentWorkController;

		public TodoListsManager(SynchronizationContext context, CurrentWorkController controller) : base(log)
		{
			guiSynchronizationContext = context;
			currentWorkController = controller;
			if (ConfigManager.IsTodoListEnabled && currentWorkController.IsOnline)
				todoListsService = new TodoListsService(guiSynchronizationContext, currentWorkController);
		}


		protected override void ManagerCallbackImpl()
		{
			if (ConfigManager.IsTodoListEnabled && currentWorkController.IsOnline)
			{
				guiSynchronizationContext.Post(x =>
				{
					if (todoListsService == null)
						todoListsService = new TodoListsService(guiSynchronizationContext, currentWorkController);
					todoListsService.BringTodoListToTop();
				}, null);
			}
		}

		protected override int ManagerCallbackInterval
		{
			get { return callbackInterval; }
		}

		public void ShowTodoList()
		{
			if (!ConfigManager.IsTodoListEnabled) return;
			guiSynchronizationContext.Post(_ =>
			{
				if (!currentWorkController.IsOnline)
				{
					log.Debug("An attempt was made to show the todolist while the user was offline.");
					MessageBox.Show(Labels.Worktime_NoResponse);
					return;
				}
				if (todoListsService == null)
					todoListsService = new TodoListsService(guiSynchronizationContext, currentWorkController);
				todoListsService.ShowTodoList();
			}, null);
		}
	}
}
