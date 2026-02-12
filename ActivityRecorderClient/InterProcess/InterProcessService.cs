using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Controller;
using System.ServiceModel;

namespace Tct.ActivityRecorderClient.InterProcess
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class InterProcessService : IInterProcessService
	{
		private readonly SynchronizationContext context;
		private readonly CurrentWorkController currentWorkController;
		private readonly MenuCoordinator menuCoordinator;
		private readonly IWindowExternalTextHelper externalTextHelper;
		private AssignData assignData;

		public InterProcessService(SynchronizationContext context, CurrentWorkController currentWorkController, MenuCoordinator menuCoordinator, IWindowExternalTextHelper externalTextHelper)
		{
			this.context = context;
			this.currentWorkController = currentWorkController;
			this.menuCoordinator = menuCoordinator;
			this.externalTextHelper = externalTextHelper;
		}

		public void AddProjectAndWorkByRule(string projectKey, string workName, string workKey, int ruleId)
		{
			context.Post((_) =>
				{
					var projectKeys = new List<string>(1) {projectKey};
					if (!currentWorkController.PermStartWorkByCompositeKey(workKey, projectKeys, true))
					{
						assignData =
							new AssignData(new AssignCompositeData
								{
									ProjectKeys = projectKeys,
									WorkKey = workKey,
									WorkName = workName,
									ServerRuleId = ruleId
								});
					}
					menuCoordinator.AssignWorkAsync(assignData);
				}, null);
		}

		public void StartWork(int workId)
		{
			var work = new WorkData { Id = workId };
			context.Post((_) => currentWorkController.PermStartWork(work, true), null);
		}

		public void StopWork()
		{
			context.Post((_) => currentWorkController.UserStopWork(), null);
		}

		public void SwitchWork(int workId)
		{
			if (currentWorkController.CurrentWorkState == WorkState.NotWorking ||
			    currentWorkController.CurrentWorkState == WorkState.NotWorkingTemp)
				throw new FaultException("NotWorking");
			var work = new WorkData { Id = workId };
			context.Post((_) => currentWorkController.PermStartWork(work), null);
		}

		internal void UpdateMenu(ClientMenu clientMenu)
		{
			// invoked on gui thread
			if (assignData != null)
			{
				if (currentWorkController.PermStartWorkByCompositeKey(assignData.Composite.WorkKey, assignData.Composite.ProjectKeys, true))
					assignData = null;
			}
		}

		public void AddExtText(string text)
		{
			externalTextHelper.AddTextToCurrentWindow(text);
		}
	}
}
