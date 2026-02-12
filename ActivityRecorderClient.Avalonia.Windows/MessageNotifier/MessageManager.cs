using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.MessageNotifier
{
	using Message = ActivityRecorderServiceReference.Message;

	class MessageManager : PeriodicManager
	{
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#if DEBUG || DEV
		private const int callbackInterval = 1 * 30 * 1000;
#else
		private const int callbackInterval = 5 * 60 * 1000;  //5 mins /**/2 ManageMeetings 115 bytes/call inside, in variables; but 60 packets 25352 bytes/call outside, in Ethernet packets
#endif

		public IMessageService MessageService { get { return messageService; } }
		
		private readonly MessageService messageService;

		public MessageManager(SynchronizationContext guiSynchronizationContext, INotificationService notificationService) : base(log)
		{
			messageService = new MessageService(guiSynchronizationContext, notificationService);
		}

		public override void Start(int firstDueTime = 0)
		{
			log.Debug("MessageManager started");
			base.Start(firstDueTime);
		}

		protected override void ManagerCallbackImpl()
		{
			messageService.GetMessagesFromServer();
		}

		

		protected override int ManagerCallbackInterval { get { return callbackInterval; } }
	}
}
