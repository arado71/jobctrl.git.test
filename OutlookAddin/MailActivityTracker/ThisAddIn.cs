using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OutlookInteropService;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Outlook = Microsoft.Office.Interop.Outlook;
using MailActivityTracker.Model;

namespace MailActivityTracker
{
	public partial class ThisAddIn
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal string Version { get; private set; }
		private ServiceHost serviceHost;
		private OutlookCaptureLib captureLib;

		public MailTrackingType TrackingType { get => captureLib.TrackingType; set => captureLib.TrackingType = value; }
		public MailTrackingSettings TrackingSettings { get => captureLib.TrackingSettings; set => captureLib.TrackingSettings = value; }

		public static MeetingPluginTaskIdSettings TaskIdSettings { get; set; } = MeetingPluginTaskIdSettings.Description;

		private void ThisAddInStartup(object sender, System.EventArgs e)
		{
			Version = Assembly.GetAssembly(typeof(ThisAddIn)).GetName().Version.ToString();
			log.Info("StartUp, version: " + Version);
			try
			{
				LocalizationHelper.InitLocalization();
				Application.NewMailEx += ApplicationOnNewMailEx;
				Application.ItemSend += ApplicationOnItemSend;
				captureLib = new OutlookCaptureLib(Application, OleHandleHelper.GetHandle);
				// don't block startup
				ThreadPool.QueueUserWorkItem(_ => StartService());
			}
			catch (Exception ex)
			{
				log.Error("Startup failed", ex);
			}
		}

		private void ApplicationOnItemSend(object item, ref bool cancel)
		{
			if (!(item is Outlook.MailItem mailItem)) return;
			captureLib.ApplicationOnItemSend(mailItem);
		}
		#region Mailing
		private void ApplicationOnNewMailEx(string anEntryId)
		{
			captureLib.ApplicationOnNewMailEx(anEntryId);
		}

		#endregion

		public MailCaptures GetMailCaptures()
		{
			return captureLib.GetMailCaptures();
		}

		private void ThisAddInShutdown(object sender, System.EventArgs e)
		{
			log.Info("Shutdown");
			serviceHost.Closed -= ServiceHostClosed;
			serviceHost.Close();
			serviceHost = null;
		}

		#region Service maintenance

		private void StartService()
		{
			try
			{
				var serviceInstance = new OutlookAddinMailCaptureService(this);
				serviceHost = new ServiceHost(serviceInstance);
				serviceHost.Description.Endpoints.Clear(); //use programmatic endpoint only

				if (!IsDebug || !IsElevated()) //we don't expose mex for release builds and we might not have permissions if not elevated
				{
					var metad = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
					if (metad != null) serviceHost.Description.Behaviors.Remove(metad);
				}

				serviceHost.AddServiceEndpoint("MailActivityTracker.IAddinMailCaptureService", new NetNamedPipeBinding(),
					"net.pipe://localhost/OutlookAddinMailCaptureService" + suffix);
				serviceHost.Closed += ServiceHostClosed;
				serviceHost.Open();

				log.Info("OutlookAddinMailCaptureService started successfully.");
			}
			catch (AddressAlreadyInUseException ex)
			{
				log.Error("OutlookAddinMailCaptureService already listening on the given address.", ex);
			}
			catch (Exception ex)
			{
				log.Error("Error occured while starting OutlookAddinMailCaptureService.", ex);
			}
		}

		private void ServiceHostClosed(object sender, EventArgs e)
		{
			log.Info("Service has been closed.");
			serviceHost.Closed -= ServiceHostClosed;
			serviceHost = null;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				Thread.Sleep(60000);
				StartService();
			});
		}

		private static bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}

		private static readonly bool isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;
#if DEBUG
		private const string suffix = "";
#else
		private readonly string suffix = "_" + Process.GetCurrentProcess().SessionId + "_" + OutlookAddinInstallHelper.OutlookAddinLocHash;
#endif

		private static bool IsElevated()
		{
			if (!isVistaOrLater) return true;
			// ReSharper disable once AssignNullToNotNullAttribute
			return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		}

		#endregion

		#region VSTO generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InternalStartup()
		{
			Startup += ThisAddInStartup;
			Shutdown += ThisAddInShutdown;
		}

		#endregion

		public void ApplyFilter(string[] keywords)
		{
			try
			{
				string query = null;
				if (keywords != null && keywords.Length > 0)
				{
					var sb = new StringBuilder("'" + keywords[0]);
					for (var i = 1; i < keywords.Length; i++)
						sb.Append("', '" + keywords[i]);
					sb.Append('\'');
					query = sb.ToString();
				}
				Application.ActiveExplorer().Search(query, Outlook.OlSearchScope.olSearchScopeAllOutlookItems);
				Thread.Sleep(25);//needed because on some rare occasions Outlook doesn't get activated otherwise
				Application.ActiveExplorer().Activate();
			}
			catch (Exception ex)
			{
				log.Error("Error occured while filtering on active explorer", ex);
			}
		}

		internal void UpdateMenu(ClientMenu menu, string placeHolder)
		{
			log.Info("MailActivityTracker calls UpdateMenu");
			if (menu == null) return;
			ProjectRibbon.PlaceHolder = placeHolder;
			ProjectRibbon.ClientMenu = menu;
		}

		public DateTime LastHeartbeat { get => captureLib.LastHeartbeat; set => captureLib.LastHeartbeat = value; }
	}
}
