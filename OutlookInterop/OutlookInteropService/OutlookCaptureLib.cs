using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Action = System.Action;
using System.Text.RegularExpressions;
using Redemption;
using Tct.ActivityRecorderClient.Capturing.Mail;
using MAPIFolder = Microsoft.Office.Interop.Outlook.MAPIFolder;

namespace OutlookInteropService
{
	public class OutlookCaptureLib : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string messageIdFormat = "[*{0}*]";

		private const string encodeTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string PR_SECURITY_FLAGS = "http://schemas.microsoft.com/mapi/proptag/0x6E010003";

		private const string rfc2822MessageId = "Message-ID";
		private const string rfc2822InReplyTo = "In-Reply-To";
		private const string rfc2822References = "References";

		private static readonly Random random = new Random();
		private static readonly Regex grabRefRegex = new Regex("<[^>]*>", RegexOptions.Singleline);
		private static readonly Regex messageIdCaptureRegex = new Regex("\\[[*]([^*]+)[*]\\]");
		private const string messageIdCapturePattern = "\\[[*]([^*]+)[*]\\]";
		private static readonly Regex jcIdRegEx = new Regex(messageIdCapturePattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

		private readonly Application application;
		private readonly Action<Action> contextPost;
		private readonly bool useRedemption;
		private readonly Func<object, IntPtr> getHandle;
		private readonly object lockObject = new object();
		private readonly List<OutlookExplorer> explorerWindows = new List<OutlookExplorer>();
		private readonly List<OutlookInspector> inspectorWindows = new List<OutlookInspector>();
		private Explorers explorers;
		private Inspectors inspectors;
		private MailTrackingType trackingType;
		private MailTrackingSettings trackingSettings = MailTrackingSettings.None;

		public MailTrackingType TrackingType
		{
			get => trackingType;
			set
			{
				if (trackingType == value) return;
				trackingType = value;
				log.Debug("TrackingType set: " + trackingType);
				RereadWindows();
			}
		}

		public MailTrackingSettings TrackingSettings
		{
			get => trackingSettings;
			set
			{
				if (trackingSettings == value) return;
				trackingSettings = value;
				log.Debug("TrackingSettings set: " + trackingSettings);
				RereadWindows();
			}
		}

		public OutlookCaptureLib(Application outlookApp, Func<object, IntPtr> getHandleFunc, Action<Action> contextPost = null, bool useRedemption = false)
		{
			application = outlookApp;
			var lockObj = new object();
			this.contextPost = contextPost ?? (action =>
			{
				lock (lockObject) action();
			});
			this.useRedemption = useRedemption;
			getHandle = getHandleFunc;
			SetupApp();
		}

		private void RereadWindows()
		{
			lock (lockObject)
				foreach (var windowWrapper in inspectorWindows.Union(explorerWindows.Cast<OutlookWindowWrapper>()))
				{
					windowWrapper.RereadMail();
				}
		}

		private void SetupApp()
		{
			log.Debug("SetupApp started");
			try
			{
				explorers = application.Explorers;
				inspectors = application.Inspectors;
				explorers.NewExplorer += ExplorersOnNewExplorer;
				foreach (Explorer explorer in explorers)
				{
					AddExplorer(explorer);
				}

				inspectors.NewInspector += InspectorsOnNewInspector;
				foreach (Inspector inspector in inspectors)
				{
					AddInspector(inspector);
				}
			}
			catch (Exception ex)
			{
				log.Error("fail", ex);
			}
			log.Debug("SetupApp finished");
		}

		public void ReleaseApp(bool disposeChildComs)
		{
			if (disposeChildComs)
				foreach (var explorerWindow in explorerWindows)
				{
					ReleaseExplorerWindow(explorerWindow);
				}
			explorerWindows.Clear();

			if (explorers != null)
			{
				explorers.NewExplorer -= ExplorersOnNewExplorer;
				Marshal.ReleaseComObject(explorers);
			}

			if (disposeChildComs)
				foreach (var inspectorWindow in inspectorWindows)
				{
					ReleaseInspectorWindow(inspectorWindow);
				}
			inspectorWindows.Clear();

			if (inspectors != null)
			{
				inspectors.NewInspector -= InspectorsOnNewInspector;
				Marshal.ReleaseComObject(inspectors);
			}

		}

		public void Dispose()
		{
			ReleaseApp(true);
		}

		private void AddExplorer(Explorer explorer)
		{
			var window = new OutlookExplorer(explorer, getHandle(explorer), GetJcIdFromMailItem, contextPost, useRedemption);
			window.Close += ExplorerWindowClose;
			window.SelectionChange += ExplorerWindowSelectionChange;
			explorerWindows.Add(window);
			log.Info($"Explorer added.");
		}

		private void ExplorerWindowClose(object sender, EventArgs e)
		{
			try
			{
				if (!(sender is OutlookExplorer window)) return;
				contextPost(() =>
				{
					lock (lockObject)
						explorerWindows.Remove(window);
					ReleaseExplorerWindow(window);
				});
			}
			catch (Exception ex)
			{
				log.Error("ExplorerWindowClose failed", ex);
			}
		}

		private void ReleaseExplorerWindow(OutlookExplorer window)
		{
			window.SelectionChange -= ExplorerWindowSelectionChange;
			window.Close -= ExplorerWindowClose;
			window.Dispose();
			log.Info($"Explorer removed.");
		}

		private void ExplorerWindowSelectionChange(OutlookExplorer explorer, MailItem mailItem, bool isOutboxSelected)
		{
			try
			{
				if (TrackingType == MailTrackingType.Disable || HeartbeatLost) return;
				if (mailItem != null)
					ProcessMailItem(mailItem, isOutboxSelected);
			}
			catch (Exception ex)
			{
				log.Error("ExplorerWindowSelectionChange failed", ex);
			}
		}

		private void ExplorersOnNewExplorer(Explorer explorer)
		{
			try
			{
				contextPost(() =>
				{
					lock (lockObject)
						if (explorerWindows.All(e => e.Window != explorer))
						{
							AddExplorer(explorer);
						}
				});
			}
			catch (Exception ex)
			{
				log.Error("ExplorersOnNewExplorer failed", ex);
			}
		}

		private void InspectorsOnNewInspector(Inspector inspector)
		{
			try
			{
				contextPost(() =>
				{
					lock (lockObject)
						if (inspectorWindows.All(e => e.Window != inspector))
						{
							AddInspector(inspector);
						}
				});
			}
			catch (Exception ex)
			{
				log.Error("InspectorsOnNewInspector failed", ex);
			}
		}

		private void AddInspector(Inspector inspector)
		{
			try
			{
				if (inspector.CurrentItem == null)
				{
					log.Warn("No current item in inspector!");
					return;
				}
				OutlookInspector window = new OutlookInspector(inspector, getHandle(inspector), GetJcIdFromMailItem, contextPost, useRedemption);
				window.Close += InspectorWindowClose;
				inspectorWindows.Add(window);
				if (window.Mail != null)
					ProcessMailItem(window.Mail, false);
				log.Debug($"Inspector added.");
			}
			catch (Exception ex)
			{
				log.Error("AddInspector failed", ex);
			}
		}

		private void InspectorWindowClose(object sender, EventArgs e)
		{
			try
			{
				if (!(sender is OutlookInspector window)) return;
				contextPost(() =>
				{
					lock (lockObject)
						inspectorWindows.Remove(window);
					ReleaseInspectorWindow(window);
				});
			}
			catch (Exception ex)
			{
				log.Error("InspectorWindowClose failed", ex);
			}
		}

		private void ReleaseInspectorWindow(OutlookInspector window)
		{
			window.Close -= InspectorWindowClose;
			window.Dispose();
			log.Debug($"Inspector removed.");
		}

		public void ApplicationOnItemSend(MailItem item)
		{
			if (TrackingType == MailTrackingType.Disable || HeartbeatLost) return;
			try
			{
				ProcessMailItem(item, false);
			}
			catch (Exception e)
			{
				log.Error("ApplicationOnItemSend failed", e);
			}
			//finally
			//{
			//	// not necessary?
			//	// Marshal.ReleaseComObject(item);
			//}
		}

		public void ApplicationOnNewMailEx(string anEntryId)
		{
			if (TrackingType == MailTrackingType.Disable || HeartbeatLost) return;
			NameSpace outlookNs = null;
			MAPIFolder folder = null;
			dynamic obj = null;
			try
			{
				outlookNs = application.GetNamespace("MAPI");
				obj = outlookNs.GetItemFromID(anEntryId);
				if (obj is MailItem)
				{
					var mailItem = (MailItem)obj;
					ProcessMailItem(mailItem, false);
				}
				else
					log.Debug("ApplicationOnNewMailEx cast");
			}
			catch (Exception e)
			{
				log.Error("ApplicationOnNewMailEx failed", e);
			}
			finally
			{
				if (obj != null) Marshal.ReleaseComObject(obj);
				if (folder != null) Marshal.ReleaseComObject(folder);
				if (outlookNs != null) Marshal.ReleaseComObject(outlookNs);
			}
		}

		private void ProcessMailItem(MailItem mailItem, bool isInOutbox)
		{
			if (mailItem != null && !isInOutbox && !string.IsNullOrEmpty(mailItem.EntryID)) // check if not new (empty) mail
			{
				if (!TrackingSettings.HasFlag(MailTrackingSettings.WriteIdToMail)) return;
				if (useRedemption)
				{
					if (safeMailItem == null)
					{
						safeMailItem = RedemptionLoader.new_SafeMailItem();
					}
					safeMailItem.Item = mailItem;
				} 
				var hashmid = CreateMailId(mailItem);
				var contentSignedFlags = useRedemption ? safeMailItem.Fields[0x6E010003] : mailItem.PropertyAccessor.GetProperty(PR_SECURITY_FLAGS);
				if (contentSignedFlags != 0)
				{
					log.Debug("Couln't process mail, maybe it's digitally signed.");
					return;
				}

				var body = mailItem.BodyFormat == OlBodyFormat.olFormatHTML
					? useRedemption ? safeMailItem.HTMLBody : mailItem.HTMLBody
					: useRedemption ? safeMailItem.Body : mailItem.Body;
				Match matchBody = body != null ? messageIdCaptureRegex.Match(body) : null;
				var presentInBody = matchBody != null && matchBody.Success;
				string idInBody = null, idInSubject = null;
				bool needRebuild = false;
				if (presentInBody)
					idInBody = matchBody.Groups[1].Value;

				var subject = mailItem.Subject;
				Match matchSubject = subject != null ? messageIdCaptureRegex.Match(subject) : null;
				var presentInSubject = matchSubject != null && matchSubject.Success;
				if (presentInSubject)
					idInSubject = matchSubject.Groups[1].Value;

				if (TrackingType == MailTrackingType.BodyOnly && (idInBody != hashmid || idInSubject != null))
					needRebuild = true;

				if (TrackingType == MailTrackingType.BodyAndSubject && (idInBody != hashmid || idInSubject != hashmid))
					needRebuild = true;

				if (!needRebuild) return;
				if (presentInSubject)
				{
					if (presentInBody && TrackingType == MailTrackingType.BodyAndSubject)
						subject = messageIdCaptureRegex.Replace(subject, string.Format(messageIdFormat, hashmid));
					else
						hashmid = matchSubject.Groups[1].Value;
				}
				else if (TrackingType == MailTrackingType.BodyAndSubject)
				{
					if (subject != null)
						subject += " " + string.Format(messageIdFormat, hashmid);
					else
						subject = string.Format(messageIdFormat, hashmid);
				}
				var isBodyChanged = false;
				if (matchBody == null || !matchBody.Success)
				{
					switch (mailItem.BodyFormat)
					{
						case OlBodyFormat.olFormatHTML:
							var htmlBody = useRedemption ? safeMailItem.HTMLBody : mailItem.HTMLBody;
							if (htmlBody != null)
								mailItem.HTMLBody = htmlBody + "\n<p>" + string.Format(messageIdFormat, hashmid) + "</p>";
							else
								mailItem.HTMLBody = "<p>" + string.Format(messageIdFormat, hashmid) + "</p>";
							isBodyChanged = true;
							break;
						case OlBodyFormat.olFormatRichText:
							// rich format body not changed because it's too complex
							break;
						default:
							var pbody = useRedemption ? safeMailItem.Body : mailItem.Body;
							if (pbody != null)
								mailItem.Body = pbody + "\n" + string.Format(messageIdFormat, hashmid);
							else
								mailItem.Body = string.Format(messageIdFormat, hashmid);
							isBodyChanged = true;
							break;
					}
				}
				if (!isBodyChanged && mailItem.Subject == subject) return;
				mailItem.Subject = subject;
				mailItem.Save();
			}
		}

		private SafeMailItem safeMailItem;
		internal string CreateMailId(MailItem mailItem)
		{
			string safeHeaders = null;
			if (useRedemption)
			{
				if (safeMailItem == null) safeMailItem = RedemptionLoader.new_SafeMailItem();
				safeMailItem.Item = mailItem;
				safeHeaders = safeMailItem.Fields[(int)MAPITags.PR_TRANSPORT_MESSAGE_HEADERS] as string ?? "";
			}
			var mailId = mailItem.Headers(rfc2822MessageId, safeHeaders);
			var references = mailItem.Headers(rfc2822References, safeHeaders);
			var inreplyto = mailItem.Headers(rfc2822InReplyTo, safeHeaders);
			ulong id;
			if (references.Length > 0 && !string.IsNullOrEmpty(references[0]))
			{
				var matcher = grabRefRegex.Match(references[0]);
				id = GetInt64HashCode(matcher.Value);
			}
			else if (inreplyto.Length > 0 && !string.IsNullOrEmpty(inreplyto[0]))
				id = GetInt64HashCode(inreplyto[0]);
			else if (mailId.Length > 0 && !string.IsNullOrEmpty(mailId[0]))
				id = GetInt64HashCode(mailId[0]);
			else
			{
				byte[] buf = new byte[8];
				random.NextBytes(buf);
				id = BitConverter.ToUInt64(buf, 0);
			}
			return Encode(id);
		}
		private static string Encode(ulong id)
		{
			var rest = id;
			var encoded = new StringBuilder();
			var tableSize = (uint)encodeTable.Length;
			while (rest > 0)
			{
				var index = (int)(rest % tableSize);
				encoded.Append(encodeTable[index]);
				rest /= tableSize;
			}
			return encoded.ToString();
		}

		// http://www.codeproject.com/Articles/34309/Convert-String-to-64bit-Integer
		static ulong GetInt64HashCode(string strText)
		{
			ulong hashCode = 0;
			if (!string.IsNullOrEmpty(strText))
			{
				//Unicode Encode Covering all characterset
				byte[] byteContents = Encoding.Unicode.GetBytes(strText);
				System.Security.Cryptography.SHA256 hash =
				new System.Security.Cryptography.SHA256CryptoServiceProvider();
				byte[] hashText = hash.ComputeHash(byteContents);
				//32Byte hashText separate
				//hashCodeStart = 0~7  8Byte
				//hashCodeMedium = 8~23  8Byte
				//hashCodeEnd = 24~31  8Byte
				//and Fold
				var hashCodeStart = BitConverter.ToUInt64(hashText, 0);
				var hashCodeMedium = BitConverter.ToUInt64(hashText, 8);
				var hashCodeEnd = BitConverter.ToUInt64(hashText, 24);
				hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
			}
			return (hashCode);
		}

		private string GetJcIdFromMailItem(OutlookItem currentItem, string safeBody)
		{
			if (!TrackingSettings.HasFlag(MailTrackingSettings.ReadId) || currentItem.IsInOutbox) return null;
			var match = jcIdRegEx.Match(currentItem.Subject);
			if (!match.Success || !match.Groups[1].Success)
			{
				match = jcIdRegEx.Match(safeBody ?? currentItem.Body);
				if (!match.Success || !match.Groups[1].Success)
				{
					if (currentItem.InnerObject is MailItem mailItem)
						return CreateMailId(mailItem);
					return null;
				}
			}
			return match.Groups[1].Value;
		}


		public MailCaptures GetMailCaptures()
		{
			var sw = Stopwatch.StartNew();
			MailCaptures result = null;
			try
			{
				Dictionary<int, MailCapture> mailCaptureByHWnd;
				lock (lockObject)
					mailCaptureByHWnd = inspectorWindows.Union(explorerWindows.Cast<OutlookWindowWrapper>()).Where(w => w.MailCapture != null).Select(w => new { w.Handle, Mail = w.MailCapture }).ToDictionary(i => i.Handle.ToInt32(), i => i.Mail);

				result = new MailCaptures { MailCaptureByHWnd = mailCaptureByHWnd.Count > 0 ? mailCaptureByHWnd : null, IsSafeMailItemCommitUsable = OutlookWindowWrapper.IsSafeMailItemCommitUsable };
				return result;
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -2146959355 || ex.ErrorCode == -2147221021) throw;
				log.Error("GetMailCaptures failed", ex);
				return new MailCaptures { IsSafeMailItemCommitUsable = OutlookWindowWrapper.IsSafeMailItemCommitUsable };
			}
			catch (Exception ex)
			{
				log.Error("GetMailCaptures failed", ex);
				return new MailCaptures { IsSafeMailItemCommitUsable = OutlookWindowWrapper.IsSafeMailItemCommitUsable };
			}
			//finally
			//{
			//	log.DebugFormat("GetMailCaptures {0} finished in {1:0.000}ms", result, sw.Elapsed.TotalMilliseconds);
			//}

		}

		private bool lastHeartbeatStatus = true;

		private bool HeartbeatLost
		{
			get
			{
				var lostNow = DateTime.UtcNow - LastHeartbeat > new TimeSpan(0, 0, 10);
				if (lostNow != lastHeartbeatStatus)
				{
					lastHeartbeatStatus = lostNow;
					log.Info(lostNow ? "Heartbeat from client lost - stopping activities" : "Heartbeat from client received - (re)starting activities");
				}
				return lostNow;
			}
		}

		public DateTime LastHeartbeat { get; set; }
	}
}
