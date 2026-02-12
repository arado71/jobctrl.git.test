using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;
using log4net;
using Tct.ActivityRecorderClient.View;
using IMailCaptureService = Tct.ActivityRecorderClient.Capturing.Mail.IMailCaptureService;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public abstract class PluginMailBase : PluginMailWithIssueSupport, ICaptureExtension, IDisposable
	{

		private readonly ILog log;

		private const string KeyFrom = "From";
		private const string KeyTo = "To";
		private const string KeyCc = "Cc";
		private const string KeyRecipients = "Recipients";
		protected const string KeyFromEmail = "FromEmail";
		private const string KeyToEmail = "ToEmail";
		private const string KeyCcEmail = "CcEmail";
		protected const string KeyRecipientsEmail = "RecipientsEmail";
		protected const string KeySubject = "Subject";
		protected const string KeyId = "Id";

		private readonly IMailCaptureService mailCaptureService;
		private volatile bool isIssueMgrEnabled;
		private IssueManager issueManager;

		protected PluginMailBase(IMailCaptureService mailCaptureService, ILog log)
		{
			this.log = log;
			log.Debug("mailCaptureService init");
			this.mailCaptureService = mailCaptureService;
			this.mailCaptureService.Initialize();
            InitIssueManager();
		}

		public override string Id { get; }

		public override IEnumerable<string> GetParameterNames()
		{
			yield break;
		}

		public override void SetParameter(string name, string value)
		{
		}

		public override IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyFrom;
			yield return KeyTo;
			yield return KeyCc;
			yield return KeyRecipients;
			yield return KeyFromEmail;
			yield return KeyToEmail;
			yield return KeyCcEmail;
			yield return KeyRecipientsEmail;
			yield return KeySubject;
			yield return KeyId;
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			//instead of looking at processname(s) we match by hWnd
			var mails = mailCaptureService.GetMailCaptures();
			MailCapture mail;
			if (mails == null || mails.MailCaptureByHWnd == null || !mails.MailCaptureByHWnd.TryGetValue(hWnd.ToInt32(), out mail))
			{
				return null;
			}

			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId);

			var keyTo = mail.GetToString();
			var keyToEmail = mail.GetToEmailString();
			var keyCc = mail.GetCcString();
			var keyCcEmail = mail.GetCcEmailString();
			var capturedValues = new Dictionary<string, string>
			{
				{ KeyId, mail.Id },
				{ KeySubject, mail.Subject },
				{ KeyFrom,  mail.From == null ? null : mail.From.ToString() },
				{ KeyFromEmail,  mail.From == null ? null : mail.From.Email },
				{ KeyTo, keyTo },
				{ KeyToEmail, keyToEmail },
				{ KeyCc, keyCc},
				{ KeyCcEmail, keyCcEmail },
				{ KeyRecipients, string.IsNullOrEmpty(keyTo) ? keyCc : (keyTo + (string.IsNullOrEmpty(keyCc) ? "" : ";" + keyCc))},
				{ KeyRecipientsEmail, SortRecipientsEmails(keyToEmail, keyCcEmail) },
			};
			return ExtendCaptureWithIssueData(capturedValues, hWnd, mail);
		}

		public static IEnumerable<KeyValuePair<string, string>> SortRecipientsEmails(IEnumerable<KeyValuePair<string, string>> capture)
		{
			if (capture == null) return null;
		    var dict = capture.ToDictionary(s => s.Key, s => s.Value);
			if (dict != null)
			{
				string recipientsEmail;
				if (dict.TryGetValue(KeyRecipientsEmail, out recipientsEmail))
				{
					dict[KeyRecipientsEmail] = SortRecipientsEmails(recipientsEmail);
				}
				return dict;
			}
			else
			{
				Debug.Fail("I don't think this is possible, but we make sure to handle it");
				return SortRecipientsEmailsEnumerator(capture);
			}
		}

		private static IEnumerable<KeyValuePair<string, string>> SortRecipientsEmailsEnumerator(IEnumerable<KeyValuePair<string, string>> capture)
		{
			foreach (var keyValuePair in capture)
			{
				if (keyValuePair.Key == KeyRecipientsEmail)
				{
					yield return new KeyValuePair<string, string>(keyValuePair.Key, SortRecipientsEmails(keyValuePair.Value));
				}
				else
				{
					yield return keyValuePair;
				}
			}
		}

		public static string SortRecipientsEmails(string toEmail, string ccEmail)
		{
			return SortRecipientsEmails(string.IsNullOrEmpty(toEmail)
				? ccEmail
				: (string.IsNullOrEmpty(ccEmail) ? toEmail : toEmail + ";" + ccEmail));
		}

		private static readonly char[] splitChars = new[] { ';' };
		public static string SortRecipientsEmails(string recipientsEmail)
		{
			if (string.IsNullOrEmpty(recipientsEmail)) return recipientsEmail;
			var emails = recipientsEmail.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
			if (emails.Length == 0) return null;
			Array.Sort(emails, StringComparer.OrdinalIgnoreCase);
			var sb = new StringBuilder(recipientsEmail.Length);
			sb.Append(emails[0].Trim());
			for (int i = 1; i < emails.Length; i++)
			{
				sb.Append(";").Append(emails[i].Trim());
			}
			return sb.ToString();
			//return string.Join(";",
			//	recipientsEmail.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)
			//		.Select(n => n.Trim())
			//		.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
			//		.ToArray());
		}

		public void Dispose()
		{
            DisposeIssueManager();
			mailCaptureService.Dispose();
			log.Debug("mailCaptureService disp");
		}
	}
}
