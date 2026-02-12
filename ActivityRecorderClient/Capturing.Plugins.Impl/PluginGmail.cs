using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public sealed class PluginGmail: PluginMailWithIssueSupport, ICaptureExtension, IDisposable
	{
		public const string PluginId = "JobCTRL.Gmail";
		private const string KeyFromEmail = "FromEmail";
		private const string KeyRecipientsEmail = "RecipientsEmail";
		private const string KeySubject = "Subject";
		private const string KeyId = "Id";
		private const string KeyJcIdSetup = "JcIdSetup";

		private PluginDomCapture domCapture;

		public override string Id { get; }

		public override IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public override void SetParameter(string name, string value)
		{
			domCapture.SetParameter(name, value);
		}

		public PluginGmail()
		{
			Id = PluginId;
			var settings = new List<DomSettings>
			{
				new DomSettings
				{
					Key = KeyFromEmail,
					EvalString = DomCaptureHelper.GmailJsFrom,
					UrlPattern = DomCaptureHelper.GmailUrl
				},
				new DomSettings
				{
					Key = KeyRecipientsEmail,
					EvalString = DomCaptureHelper.GmailJsTo,
					UrlPattern = DomCaptureHelper.GmailUrl
				},
				new DomSettings
				{
					Key = KeySubject,
					EvalString = DomCaptureHelper.GmailJsSubject,
					UrlPattern = DomCaptureHelper.GmailUrl
				},
				new DomSettings
				{
					Key = KeyId,
					EvalString = DomCaptureHelper.GmailJsId,
					UrlPattern = DomCaptureHelper.GmailUrl
				},
				new DomSettings
				{
					Key = KeyJcId,
					EvalString = DomCaptureHelper.GmailJsJcId,
					UrlPattern = DomCaptureHelper.GmailUrl
				},
				new DomSettings
				{
					Key = KeyJcIdSetup,
					EvalString = DomCaptureHelper.GmailJCIdSetting.FS((ConfigManager.MailTrackingType>=Mail.MailTrackingType.BodyAndSubject).ToString().ToLowerInvariant(),(ConfigManager.MailTrackingType == Mail.MailTrackingType.BodyAndSubject).ToString().ToLowerInvariant()),
					UrlPattern = DomCaptureHelper.GmailUrl
				},
			};
			domCapture = new PluginDomCapture();
			domCapture.SetDomCaptures(settings);
			InitIssueManager();
		}

		public override IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			var res = domCapture.Capture(hWnd, processId, processName);
			if (res == null) return null;
			var capDict = res.ToDictionary(s => s.Key, s => s.Value);
			var mail = GetMailCaptureFromCapture(capDict);
			capDict.Remove(KeyJcId); // removing JcId from capture. It added in next step if necessary.
			res = ExtendCaptureWithIssueData(capDict.Select(d => new KeyValuePair<string, string>(d.Key, d.Value)), hWnd, mail);
			return PluginMailBase.SortRecipientsEmails(res);
		}

		public override IEnumerable<string> GetCapturableKeys()
		{
			foreach (var key in base.GetCapturableKeys())
			{
				yield return key;
			}
			yield return KeyFromEmail;
			yield return KeyRecipientsEmail;
			yield return KeySubject;
			yield return KeyId;
		}

		private MailCapture GetMailCaptureFromCapture(Dictionary<string, string> dict)
		{
			var ret = new MailCapture();
			ret.Subject = dict.GetValueOrDefault(KeySubject);
			var to = dict.GetValueOrDefault(KeyRecipientsEmail);
			ret.To = (string.IsNullOrEmpty(to) ? "" : to).Split(';').Select(s => new MailAddress() { Email = s }).ToList();
			ret.From = new MailAddress() { Email = dict.GetValueOrDefault(KeyFromEmail) };
			ret.Id = dict.GetValueOrDefault(KeyId);
			ret.JcId = dict.GetValueOrDefault(KeyJcId);
			//var match = new Regex("\\[[*]([^*]+)[*]\\]").Match(ret.Subject);
			//if (!match.Success || !match.Groups[1].Success)
			//{
			//    ret.JcId = null;
			//}
			//else
			//{
			//    ret.JcId = match.Groups[1].Value;

			//}


			return ret;

		}

		public void Dispose()
		{
            DisposeIssueManager();
			domCapture?.Dispose();
		}
	}
}
