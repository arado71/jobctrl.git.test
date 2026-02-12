using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderService.Persistence;

namespace Tct.ActivityRecorderService.EmailStats
{
	/// <summary>
	/// Thread-safe singleton class for sending out emails.
	/// This is required to work around the buggy implementation of SmtpClient which makes it unusabe
	/// when several threads use it and some messages time out.
	/// https://connect.microsoft.com/VisualStudio/feedback/details/706796/bug-of-smtpclient-causes-timeouts-and-makes-it-unusable
	/// </summary>
	public class EmailManager : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int retryType1Count = Math.Max(ConfigManager.EmailRetryType1Count, 0);
		private static readonly TimeSpan retryType1Delay = TimeSpan.FromSeconds(ConfigManager.EmailRetryType1InvervalInSec);
		private static readonly int retryType2Count = Math.Max(ConfigManager.EmailRetryType2Count, 0);
		private static readonly TimeSpan retryType2Delay = TimeSpan.FromSeconds(ConfigManager.EmailRetryType2InvervalInSec);

		private static readonly EmailPathResolver pathResolver = new EmailPathResolver(ConfigManager.EmailsToSendDir);

		public static EmailManager Instance = new EmailManager();

		private readonly FixedThreadPool senderPool = new FixedThreadPool(2);

		private EmailManager()
		{
			var messages = PersistenceHelper.LoadAllFromRootDir(pathResolver);
			foreach (var message in messages)
			{
				SendWithRetry(message);
			}
		}

		public void Send(EmailMessage message)
		{
			long length;
			PersistenceHelper.Save(pathResolver, message, out length);
			if (length > ConfigManager.EmailMaxSizeInMB * 1024 * 1024)
			{
				log.Error("Dropping too large email to " + message.To + " with subj " + message.Subject + " size " + length);
				PersistenceHelper.Delete(pathResolver, message);
				return;
			}
			SendWithRetry(message);
		}

		private void SendWithRetry(EmailMessage message, int tries = 1)
		{
			senderPool.QueueUserWorkItem(_ =>
			{
				var sw = Stopwatch.StartNew();
				try
				{
					EmailHelper.Send(message.To, message.Subject, message.PlainBody, message.HtmlBody, message.HtmlResources);
					log.Info("Successfully sent email to:" + message.To + " with subject:" + message.Subject + " try:" + tries + " in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
					PersistenceHelper.Delete(pathResolver, message);
				}
				catch (Exception ex)
				{
					var maxTries = retryType1Count + retryType2Count + 1;
					if (tries >= maxTries)
					{
						log.Error("Unable to send email to:" + message.To + " with subject:" + message.Subject + " tries (" + tries + "/" + maxTries + ") in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ", ex);
						return;
					}
					log.Warn("Unable to send email to:" + message.To + " with subject:" + message.Subject + " tries (" + tries + "/" + maxTries + ") in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ", ex);
					//retry on error
					TaskEx
						.Delay(tries <= retryType1Count ? retryType1Delay : retryType2Delay) //this is not too clever but might be enough
						.ContinueWith(ant => SendWithRetry(message, tries + 1));
				}
			});
		}

		public void Dispose()
		{
			senderPool.Dispose();
		}
	}
}
