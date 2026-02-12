using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Microsoft.Office.Interop.Outlook;
using OutlookInteropService;
using Redemption;
using Tct.ActivityRecorderClient;
using Action = System.Action;

namespace OutlookInteropService
{
	public abstract class OutlookWindowWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly CachedDictionary<string, MailCapture> mailCaptureCache = new CachedDictionary<string, MailCapture>(TimeSpan.FromMinutes(60), true);

		public static bool IsSafeMailItemCommitUsable { get; set; }

		private const string MailItemPropertyTo = "To";
		private const string MailItemPropertyCC = "CC";
		private const string MailItemPropertySubject = "Subject";

		private readonly Func<OutlookItem, string, string> GetJcIdFromMailItem;
		private readonly Func<OutlookItem, MailCapture> GetMailCaptureFunc;
		private readonly bool useRedemption;
		private string propertyTo, propertyCC, propertySubject;
		private readonly Timer delayReadTimer;
		private bool delayReadTimerStopped;
		private HashSet<Tuple<bool,string>> delayedPropSet = new HashSet<Tuple<bool, string>>();
		private SafeMailItem safeMailItem;

		public IntPtr Handle { get; protected set; }
		protected OutlookMailItemWrapper Item { get; set; }
		protected OutlookMailItemWrapper InlineItem { get; set; }
		protected readonly Action<Action> contextPost;
		public MailCapture MailCapture { get; private set; }

		public MailItem Mail => Item?.MailItem;
		public MailItem InlineMail => InlineItem?.MailItem;

		protected OutlookWindowWrapper(Func<OutlookItem, string, string> getJcIdFromMailItem, Action<Action> contextPost, bool useRedemption)
		{
			GetJcIdFromMailItem = getJcIdFromMailItem;
			this.useRedemption = useRedemption;
			this.contextPost = contextPost;
			if (useRedemption)
				GetMailCaptureFunc = GetMailCaptureFromMailItem;
			else
				GetMailCaptureFunc = GetMailCaptureForOutlookItem;
			delayReadTimer = new Timer(DelayReadTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
		}

		private MailCapture GetMailCaptureOrCached(OutlookMailItemWrapper item, bool forced = false)
		{
			if (!forced && mailCaptureCache.TryGetValue(item.EntryID, out var capture)) return capture;
			capture = GetMailCaptureFunc(item);
			mailCaptureCache.Set(item.EntryID, capture);
			return capture;
		}

		public void RereadMail()
		{
			if (Mail != null)
				MailCapture = GetMailCaptureOrCached(string.IsNullOrEmpty(SafeGetTo(InlineMail)) ? Item : InlineItem, true);
		}

		protected void Init()
		{
			if (Mail != null)
			{
				Item.PropertyChange += MailOnPropertyChange;
				MailCapture = GetMailCaptureOrCached(string.IsNullOrEmpty(SafeGetTo(InlineMail)) ? Item : InlineItem);
			}
			else
			{
				MailCapture = null;
			}
		}

		protected void DeInit(OutlookMailItemWrapper item)
		{
			if (item?.MailItem != null)
			{
				item.PropertyChange -= MailOnPropertyChange;
			}
			lock (delayReadTimer)
			{
				DisableDelayTimer();
				item?.Dispose();
				item = null;
				propertySubject = null;
				propertyTo = null;
				propertyCC = null;
			}
		}

		protected void InitInline()
		{
			if (InlineMail == null) return;
			InlineItem.PropertyChange += InlineMailOnPropertyChange;
			if (string.IsNullOrEmpty(SafeGetTo(InlineMail))) return;
			MailCapture = GetMailCaptureOrCached(InlineItem);
		}

		protected void DeInitInline()
		{
			if (InlineItem != null)
			{
				InlineItem.PropertyChange -= InlineMailOnPropertyChange;
			}
			lock(delayReadTimer)
			{
				DisableDelayTimer();
				InlineItem?.Dispose();
				InlineItem = null;
			}
		}

		protected void DisableDelayTimer()
		{
			delayReadTimerStopped = true;
			delayReadTimer.Change(Timeout.Infinite, Timeout.Infinite);
			safeMailItem = null;
		}

		private string SafeGetTo(MailItem item)
		{
			if (item == null) return null;
			if (!useRedemption) return item.To;
			var mailItem = RedemptionLoader.new_SafeMailItem();
			mailItem.Item = item;
			return mailItem.To;
		}

		private void ProcessPropertyChange(bool inline, string name)
		{
			lock (delayReadTimer)
			{
				delayReadTimerStopped = false;
				delayedPropSet.Add(Tuple.Create(inline, name));
				delayReadTimer.Change(10000, Timeout.Infinite);
			}
		}

		private void DelayReadTimerCallback(object state)
		{
			contextPost(() =>
			{
				try
				{
					var wasMail = false;
					var wasInline = false;
					List<Tuple<bool, string>> delayedPropSetCopy;
					lock (delayReadTimer)
					{
						if (delayReadTimerStopped)
							return;
						if (useRedemption)
						{
							if (safeMailItem == null)
							{
								safeMailItem = RedemptionLoader.new_SafeMailItem();
								safeMailItem.Item = InlineMail ?? Mail;
							}
						}

						delayedPropSetCopy = delayedPropSet.ToList();
						delayedPropSet.Clear();
					}

					// New function in Redemption 5.24.0.5736:
					// now expose the new Commit method. The method commits all pending changes that only exist in the Outlook internal cache
					// without actually saving the item. Previously, an Outlook object must have been saved (by calling the Save method)
					// to make pending changes made through the Outlook Object Model or Outlook UI visible to Extended MAPI and Redemption.
					// Calling Save can be undesirable as it persists the item and makes it visible to the end user (e.g. in the Drafts folder),
					// which can require the saved item to be tracked and deleted. Moreover, after calling Save ,Outlook would no longer prompt the user
					// to save the pending changes.
					// http://www.dimastr.com/redemption/history.htm#table119

					if (useRedemption && IsSafeMailItemCommitUsable)
						try
						{
							safeMailItem?.Commit();
						}
						catch (COMException)
						{
							log.Warn("ISafeMailItem.Commit failed, disabled temporary");
							IsSafeMailItemCommitUsable = false;
						}

					foreach (var item in delayedPropSetCopy)
					{
						var mailItem = item.Item1 ? InlineMail : Mail;
						if (mailItem == null) continue;
						var name = item.Item2;
						switch (name)
						{
							case MailItemPropertySubject:
								var subj = mailItem.Subject;
								if (subj == propertySubject) break;
								propertySubject = subj;
								if (item.Item1) wasInline = true;
								else wasMail = true;
								break;
							case MailItemPropertyTo:
								var to = useRedemption ? safeMailItem.To : mailItem.To;
								if (to == propertyTo) break;
								propertyTo = to;
								if (item.Item1) wasInline = true;
								else wasMail = true;
								break;
							case MailItemPropertyCC:
								string cc;
								cc = useRedemption ? safeMailItem.CC : mailItem.CC;
								if (cc == propertyCC) break;
								propertyCC = cc;
								if (item.Item1) wasInline = true;
								else wasMail = true;
								break;
						}
					}

					if (wasInline && InlineItem != null && !string.IsNullOrEmpty(propertyTo))
						try
						{
							MailCapture = GetMailCaptureOrCached(InlineItem, true);
						}
						catch (System.Exception ex)
						{
							log.Error("InlineMail properties processing failed", ex);
						}
					else if (wasMail && Item != null)
						try
						{
							MailCapture = GetMailCaptureOrCached(Item, true);
						}
						catch (System.Exception ex)
						{
							log.Error("Mail properties processing failed", ex);
						}
				}
				catch (System.Exception ex)
				{
					log.Error("Delayed property processing failed", ex);
				}
			});
		}

		private void InlineMailOnPropertyChange(string name)
		{
			if (InlineMail == null) return;
			ProcessPropertyChange(true, name);
		}

		protected void BeforeClose()
		{
			var del = Close;
			if (del != null)
				del(this, EventArgs.Empty);
		}

		public event EventHandler Close;

		private void MailOnPropertyChange(string name)
		{
			if (Mail == null) return;
			ProcessPropertyChange(false, name);
		}

		private static dynamic GetSafeMailProperty(Microsoft.Office.Interop.Outlook.MailItem mail, string name)
		{
			try
			{
				return mail.PropertyAccessor.GetProperty(name);
			}
			catch (COMException)
			{
				return null;
			}
		}

		private MailCapture GetMailCaptureForOutlookItem(OutlookItem item)
		{
			var mailCap = new MailCapture
			{
				Id = item.EntryID,
				JcId = GetJcIdFromMailItem?.Invoke(item, null),
				Subject = item.Subject,
			};
			if (item.InnerObject is MailItem mail)
			{
				if (item.IsInOutbox) return null;
				var type = mail.SenderEmailType;
				var senderEmailAddress = mail.SenderEmailAddress;
				var fromEmail = mail.PropertyAccessor != null && "EX".Equals(type) ? (string)GetSafeMailProperty(mail, PidTagSenderSmtpAddress) : senderEmailAddress;
				if (string.IsNullOrEmpty(fromEmail) && mail.PropertyAccessor != null) fromEmail = GetFromEmailFromHeaders((string)GetSafeMailProperty(mail, PidTagTransportMessageHeaders)); //cheap hax
				mailCap.From = string.IsNullOrEmpty(fromEmail) ? mail.SendUsingAccount == null ? new MailAddress() 
					: new MailAddress { Name = mail.SendUsingAccount.UserName ?? mail.SendUsingAccount.DisplayName, Email = mail.SendUsingAccount.SmtpAddress }
					: new MailAddress { Email = fromEmail, Name = mail.SenderName };
				if (mail.Recipients != null)
				{
					mailCap.To = getMailAddressesFromRecipients(mail.Recipients, Microsoft.Office.Interop.Outlook.OlMailRecipientType.olTo);
					mailCap.Cc = getMailAddressesFromRecipients(mail.Recipients, Microsoft.Office.Interop.Outlook.OlMailRecipientType.olCC);
				}
				return mailCap;
			}
			return null;
		}

		private List<MailAddress> getMailAddressesFromRecipients(Recipients recipients, Microsoft.Office.Interop.Outlook.OlMailRecipientType type)
		{
			int i = 0;
			List<MailAddress> result = new List<MailAddress>();
			foreach (Recipient recipient in recipients)
			{
				if (i++ >= 100) break;
				if (recipient.Type != (int)type) continue;
				MailAddress address;
				if (recipient.AddressEntry == null || !"EX".Equals(recipient.AddressEntry.Type))
				{
					address = new MailAddress { Name = recipient.Name, Email = recipient.Address };
				} else
				{
					try
					{
						address = new MailAddress { Name = recipient.Name, Email = recipient.AddressEntry.GetExchangeUser().PrimarySmtpAddress };
					} catch(COMException ex)
					{
						log.Warn("AddressEntry could not be opened.", ex);
						continue;
					}
				}
				result.Add(address);
			}
			return result;
		}

		private const string PidTagSenderSmtpAddress = "http://schemas.microsoft.com/mapi/proptag/0x5D01001F"; //PidTagRecipientSenderSMTPAddress_W
		private const string PidTagTransportMessageHeaders = "http://schemas.microsoft.com/mapi/proptag/0x007D001F";

		private static readonly Regex fromRegex = new Regex(@"^\s*From:\s*(.*\<(?<email>[^\<\>\s]+@[^\<\>\s]+)\>|(?<email>[^\<\>\s]+@[^\<\>\s]+))\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		private static string GetFromEmailFromHeaders(string headers)
		{
			if (string.IsNullOrEmpty(headers)) return null;
			var match = fromRegex.Match(headers);
			if (!match.Success) return null;
			var group = match.Groups["email"];
			if (!group.Success) return null;
			return group.Value;
		}

		private const int PidTagSenderSmtpAddressInt = 0x5D01001F; //http://schemas.microsoft.com/mapi/proptag/0x5D01001F //PidTagRecipientSenderSMTPAddress_W

		private MailCapture GetMailCaptureFromMailItem(OutlookItem outlookItem)
		{
			if (outlookItem.IsInOutbox) return null;

			SafeRecipients recipients = null;
			SafeRecipient recipient = null;
			try
			{
				var currentItem = outlookItem.InnerObject;
				if (safeMailItem == null)
				{
					safeMailItem = RedemptionLoader.new_SafeMailItem();
					safeMailItem.Item = currentItem;
				}

				recipients = safeMailItem.Recipients;
				var to = new List<MailAddress>();
				var cc = new List<MailAddress>();
				var count = recipients.Count;
				if (count > 100) count = 100; //maximize the count of processed items to improve performance
				for (int i = 1; i <= count; i++)
				{
					recipient = recipients.Item(i);
					switch ((OlMailRecipientType)recipient.Type)
					{
						case OlMailRecipientType.olTo:
							to.Add(GetMailAddressFromRecipient(recipient));
							break;
						case OlMailRecipientType.olCC:
							cc.Add(GetMailAddressFromRecipient(recipient));
							break;
					}
					Marshal.ReleaseComObject(recipient);
					recipient = null;
				}
				//we could also use PR_SENDER_ADDRTYPE_W instead of SenderEmailType
				var type = (string)currentItem.GetType().InvokeMember("SenderEmailType", System.Reflection.BindingFlags.GetProperty, null, currentItem, null);
				var fromEmail = "EX".Equals(type) ? (string)safeMailItem.Fields[PidTagSenderSmtpAddressInt] : safeMailItem.SenderEmailAddress;
				//mailItem.Sender.SMTPAddress //handle nulls and releases - this is quite expensive, so should be cached
				if (string.IsNullOrEmpty(fromEmail)) fromEmail = GetFromEmailFromHeaders((string)safeMailItem.Fields[(int)MAPITags.PR_TRANSPORT_MESSAGE_HEADERS]); //cheap hax

				return new MailCapture()
				{
					Id = Hash((string) currentItem.GetType().InvokeMember("EntryID", System.Reflection.BindingFlags.GetProperty, null, currentItem, null)),
					JcId = GetJcIdFromMailItem?.Invoke(outlookItem, safeMailItem.Body ?? ""),
					From = new MailAddress() { Email = fromEmail, Name = safeMailItem.SenderName },
					Subject = (string) currentItem.GetType().InvokeMember("Subject", System.Reflection.BindingFlags.GetProperty, null, currentItem, null),
					To = to,
					Cc = cc,
				};
			}
			finally
			{
				if (recipient != null) Marshal.ReleaseComObject(recipient);
				if (recipients != null) Marshal.ReleaseComObject(recipients);
				//if (mailItem != null) Marshal.ReleaseComObject(mailItem);
			}
		}

		private static MailAddress GetMailAddressFromRecipient(SafeRecipient recipient)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(recipient.Name + ": " + (recipient.Resolved ? "Resolved" : "Not Resolved"));
			}
			var result = new MailAddress() { Name = recipient.Name };
			if ("EX".Equals(recipient.Fields[(int)MAPITags.PR_AddrType])) //recipient.AddressEntry.Type would throw if no network connection
			{
				result.Email = (string)recipient.Fields[(int)MAPITags.PR_SMTP_ADDRESS];
				//recipient.AddressEntry.SMTPAddress //handle nulls and releases
			}
			else
			{
				result.Email = recipient.Address;
			}
			return result;
		}

		private static string Hash(string text)
		{
			return text == null ? null : Convert.ToBase64String(Tct.ActivityRecorderClient.Murmur3.ComputeHash(Encoding.UTF8.GetBytes(text)));
		}

		public virtual void Dispose()
		{
			delayReadTimer.Dispose();
			Item?.Dispose();
			InlineItem?.Dispose();
		}
	}
}
