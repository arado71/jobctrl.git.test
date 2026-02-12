using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.MessageNotifier
{
	using Message = Tct.ActivityRecorderClient.ActivityRecorderServiceReference.Message;
	class MessageService : IMessageService
	{
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string MessageNotifierKey = "MessageNotifier";

		private ObservableCollection<Message> messages = null;
		private readonly MessageView messageView;
		private readonly SynchronizationContext context;
		private readonly INotificationService notificationService;
		private DateTime? lastMessageLastChangeDate = null;
		private object lockObject = new object();
		private int notificationCount = 0;
		private Message lastShowedMessage;


		public MessageService(SynchronizationContext guiSynchronizationContext, INotificationService notificationService)
		{
			context = guiSynchronizationContext;
			this.notificationService = notificationService;
			messageView = new MessageView(this);
			ThreadPool.QueueUserWorkItem(_ => getMessagesFromStorage());
		}

		public void GetMessagesFromServer()
		{
			lock (lockObject)
			{
				try
				{
					hideNotificationIfNecessary();
				}
				catch (Exception e)
				{
					log.Error("Unexpected error during hiding notifications.", e);
				}
				try
				{
					notificationCount = 0;
					if (messages == null) return;
					int userId = ConfigManager.UserId;
					List<Message> messagesFromServer = ActivityRecorderClientWrapper.Execute(n =>
						n.GetMessages(userId, lastMessageLastChangeDate, ConfigManager.EnvironmentInfo.ComputerId));

					foreach (var message in messagesFromServer)
					{
						messages.Add(message);
						messageView.AddMessage(new DisplayedMessage(message));
						updateLastChangeDateIfNecessary(message);
					}
				}
				catch (Exception e)
				{
					log.Debug("Unable to get messages from the server.", e);
				}
			}
		}

		private void updateLastChangeDateIfNecessary(Message message)
		{
			if (!lastMessageLastChangeDate.HasValue || lastMessageLastChangeDate < message.CreatedAt)
			{
				lastMessageLastChangeDate = message.CreatedAt;
			}
			if (message.LastUpdatedAt.HasValue && message.LastUpdatedAt > lastMessageLastChangeDate)
			{
				lastMessageLastChangeDate = message.LastUpdatedAt;
			}
		}

		private void sendNotification(Message message)
		{
			context.Post(_ =>
			{
				notificationService.HideNotification(MessageNotifierKey);
				notificationService.ShowNotification(MessageNotifierKey, TimeSpan.Zero,
					Labels.NotificationNewMessageTitle, message.ContentWithoutFormatting, null, () => messageView.ShowWithId(message.Id));
			}, null);
			lastShowedMessage = message;
		}

		private void hideNotificationIfNecessary()
		{
			if (lastShowedMessage != null && lastShowedMessage.ExpiryInHours > 0 &&
				((lastShowedMessage.LastUpdatedAt ?? lastShowedMessage.CreatedAt)
					.AddHours(lastShowedMessage.ExpiryInHours) < DateTime.UtcNow
					|| lastShowedMessage.PCLastReadAt != null
					|| lastShowedMessage.MobileLastReadAt != null)
				)
			{
				context.Post(_ =>
				{
					if (notificationService.IsActive(MessageNotifierKey))
						notificationService.HideNotification(MessageNotifierKey);
				}, null);
				lastShowedMessage = null;
			}
		}

		private void getMessagesFromStorage()
		{
			lock (lockObject)
			{
				messages = new ObservableCollection<Message>();
				foreach (var message in IsolatedStorageMessagesHelper.Items)
				{
					if (messages.Count == 0)
					{
						context.Post(_ =>
						{
							((Platform.PlatformWinFactory)Platform.Factory).MainForm.AddEtcExtraMenuitem(() => Labels.Messages, ShowMessages);
						}, null);
					}
					messages.Add(message);
					updateLastChangeDateIfNecessary(message);
					sendNotificationIfNeeded(message);
					messageView.AddMessage(new DisplayedMessage(message));
				}

				messages.CollectionChanged += OnMessageCollectionChanged;
			}
		}

		public void ShowMessages()
		{
			context.Post(_ =>
			{
				messageView.ShowWithId();
			}, null);
		}

		private void sendNotificationIfNeeded(Message message)
		{
			if ((message.LastUpdatedAt ?? message.CreatedAt).AddHours(message.ExpiryInHours == 0 ? 168 : message.ExpiryInHours) < DateTime.UtcNow) return;
			if (message.PCLastReadAt != null || message.MobileLastReadAt != null) return;
			if (notificationCount++ > 3) return;
			sendNotification(message);
		}

		private void OnMessageCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				if ((e.OldItems == null || e.OldItems.Count == 0) && e.NewItems.Count == messages.Count)
				{
					context.Post(_ =>
					{
						((Platform.PlatformWinFactory)Platform.Factory).MainForm.AddEtcExtraMenuitem(() => Labels.Messages, ShowMessages);
					}, null);
				}
				foreach (var item in e.NewItems)
				{
					Message message = (Message)item;
					sendNotificationIfNeeded(message);
					IsolatedStorageMessagesHelper.Save(message);
				}

			}
		}

		public DateTime? SetPCReadAt(int messageId)
		{
			DateTime? returnDate = null;
			try
			{
				Message message = messages.FirstOrDefault(m => m.Id == messageId);
				if (message == null) return null;
				returnDate = ActivityRecorderClientWrapper.Execute(n =>
					n.MarkMessageAsRead(message.TargetUserId, messageId, ConfigManager.EnvironmentInfo.ComputerId));
				message.PCLastReadAt = returnDate;
				IsolatedStorageMessagesHelper.Save(message);
				return returnDate;
			}
			catch (Exception e)
			{
				log.Debug("Unable to set messages as read.", e);
				return returnDate;
			}
		}
	}
}
