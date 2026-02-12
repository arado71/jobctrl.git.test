using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class UserDisplay : UserControl, IDropdownContainer, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<IDropdown> dropdowns = new List<IDropdown>();
		private readonly CustomDropdown<string> webContextMenu, etcMenu;
		private readonly List<DropdownElement> etcMenuDefaultElements, etcMenuDefaultAndHiddenElements;
		private string userId;
		private string username;
		private Dictionary<IntPtr, DropdownElement> etcExtraElements = new Dictionary<IntPtr, DropdownElement>();
		private readonly DropdownElement deMsProjectUpload;
		private readonly DropdownElement deWorkDetectorRules;
		private readonly DropdownElement deErrorReporting;
		private readonly DropdownElement deHelp;
		private readonly DropdownElement deChangeUser;

		public event EventHandler PreferenceClick;
		public event EventHandler QuitClick;
		public event EventHandler UserChangeClick;
		public event EventHandler HelpClick;
		public event EventHandler RulesClick;
		public event EventHandler ErrorReportClick;
		public event EventHandler ProjectUploadClick;

		public string UserName
		{
			get { return username; }

			set
			{
				username = value;
				RenderText();
			}
		}

		public Form MainForm { get; set; }

		public string UserId
		{
			get { return userId; }

			set
			{
				userId = value;
				RenderText();
			}
		}

		public UserDisplay()
		{
			InitializeComponent();
			SetColorScheme();
			webContextMenu = new CustomDropdown<string>(this);
			etcMenu = new CustomDropdown<string>(this);
			deMsProjectUpload = new DropdownElement(Labels.Menu_MsProjectUpload + @"...", () =>
			{
				log.Debug("UI - MsProject upload dropdown clicked");
				Raise(ProjectUploadClick);
			});
			deWorkDetectorRules = new DropdownElement(Labels.Menu_WorkDetectorRules + @"...", () =>
			{
				log.Debug("UI - Work detector dropdown clicked");
				Raise(RulesClick);
			});
			deErrorReporting = new DropdownElement(Labels.Menu_ErrorReporting + @"...", () =>
			{
				log.Debug("UI - Error reporting dropdown clicked");
				Raise(ErrorReportClick);
			});
			deChangeUser = new DropdownElement(Labels.Menu_ChangeUser, () =>
			{
				log.Debug("UI - Change user clicked");
				Raise(UserChangeClick);
			}) { Visible = false };
			deHelp = new DropdownElement(Labels.Menu_Help, () =>
			{
				log.Debug("UI - Help clicked");
				Raise(HelpClick);
			});
			etcMenuDefaultElements = new List<DropdownElement> { deWorkDetectorRules, deChangeUser, deHelp };
			etcMenuDefaultAndHiddenElements = new List<DropdownElement> { deWorkDetectorRules, deErrorReporting, deChangeUser, deHelp };
			webContextMenu.Hidden += HandleHidden;
			etcMenu.Hidden += HandleHidden;
			RecentUrlQuery.Instance.RecentChanged += HandleRecentUrlChanged;
			pQuit.Enabled = false;
			Localize();
		}

		public void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				label1.BackColor = SystemColors.Window;
				label1.ForeColor = SystemColors.WindowText;
				label1.ForeColorAlternative = SystemColors.HighlightText;
				this.pWeb.BackColor = System.Drawing.Color.White;
				this.pPreference.BackColor = System.Drawing.Color.White;
				this.pQuit.BackColor = System.Drawing.Color.White;
				this.pEtc.BackColor = System.Drawing.Color.White;
				BackColor = SystemColors.Window;
			}
			else
			{
				label1.BackColor = StyleUtils.BackgroundInactive;
				label1.ForeColor = Color.FromArgb(77, 77, 77);
				label1.ForeColorAlternative = StyleUtils.ForegroundLight;
				this.pWeb.BackColor = Color.Transparent;
				this.pPreference.BackColor = Color.Transparent;
				this.pQuit.BackColor = Color.Transparent;
				this.pEtc.BackColor = Color.Transparent;
				BackColor = StyleUtils.BackgroundInactive;
			}
		}

		public void Localize()
		{
			deMsProjectUpload.Value = Labels.Menu_MsProjectUpload + @"...";
			deWorkDetectorRules.Value = Labels.Menu_WorkDetectorRules + @"...";
			deErrorReporting.Value = Labels.Menu_ErrorReporting + @"...";
			deChangeUser.Value = Labels.Menu_ChangeUser;
			deHelp.Value = Labels.Menu_Help;
			toolTip1.SetToolTip(pPreference, Labels.Menu_Preferences);
			toolTip1.SetToolTip(pQuit, Labels.Menu_Exit);
			toolTip1.SetToolTip(pEtc, Labels.Menu_Etc);
			SetUrlTooltip();
			RecentUrlQuery.Instance.Localize();
			webContextMenu.Populate(
				RecentUrlQuery.Instance.GetNames().Select(urlName => (SelectableControl<string>)
					new DropdownCheckElement(urlName, () =>
					{
						log.DebugFormat("UI - Web dropdown element clicked ({0})", urlName);
						RecentUrlQuery.Instance.SetLink(urlName);
						NavigateWeb();
					}))
			);
			foreach (var element in etcExtraElements.Values)
			{
				if (element.Tag is Func<string> textAccessor) element.Value = textAccessor();
			}
		}

		public long AddEtcExtraMenuitem(Func<string> textAccessor, Action clickHandler)
		{
			var text = textAccessor();
			var deExtra = new DropdownElement(text, () =>
			{
				log.Debug("UI - Extra menuitem (" + text + ") clicked");
				clickHandler();
			});
			deExtra.Tag = textAccessor;
			etcMenuDefaultElements.Add(deExtra);
			etcMenuDefaultAndHiddenElements.Add(deExtra);
			etcExtraElements.Add(deExtra.Handle, deExtra);
			return deExtra.Handle.ToInt64();
		}

		public void RemoveEtcExtraMenuitem(long menuid)
		{
			var handle = (IntPtr) menuid;
			DropdownElement deExtra;
			if (!etcExtraElements.TryGetValue(handle, out deExtra)) return;
			etcMenuDefaultElements.Remove(deExtra);
			etcMenuDefaultAndHiddenElements.Remove(deExtra);
			etcExtraElements.Remove(handle);
		}

		public bool DropdownShown
		{
			get { return dropdowns.Any(x => x.IsShown); }
		}

		public bool QuitEnabled
		{
			get
			{
				return pQuit.Enabled;
			}
			set
			{
				if (value == pQuit.Enabled) return;
				pQuit.Enabled = value;
				deChangeUser.Visible = value;
			}
		}

		public bool WebVisible
		{
			set
			{
				pWeb.Visible = value;
			}
		}

		public void RegisterDropdown(IDropdown dropdown)
		{
			dropdowns.Add(dropdown);
		}

		public event EventHandler DropdownClosed;

		protected void Raise(EventHandler handler)
		{
			EventHandler evt = handler;
			if (evt != null)
			{
				evt(this, EventArgs.Empty);
			}
		}

		private void HandleHidden(object sender, EventArgs e)
		{
			EventHandler evt = DropdownClosed;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		private void EnsureDropdownVisibility(DropdownElement element, bool isVisible)
		{
			if (isVisible)
			{
				if (!etcMenuDefaultElements.Contains(element))
				{
					etcMenuDefaultElements.Add(element);
				}

				if (!etcMenuDefaultAndHiddenElements.Contains(element))
				{
					etcMenuDefaultAndHiddenElements.Add(element);
				}
			}
			else
			{
				if (etcMenuDefaultElements.Contains(element))
				{
					etcMenuDefaultElements.Remove(element);
				}

				if (etcMenuDefaultAndHiddenElements.Contains(element))
				{
					etcMenuDefaultAndHiddenElements.Remove(element);
				}
			}
		}

		private void HandleEtcClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
			{
				var isProjectSyncEnabled = !string.IsNullOrEmpty(ConfigManager.MsProjectAddress);
#if ProjectSync
				EnsureDropdownVisibility(deMsProjectUpload, isProjectSyncEnabled);
#endif
				if ((ModifierKeys & Keys.Shift) != 0)
				{
					log.Debug("UI - Etc clicked (modifier)");
					etcMenu.Populate(etcMenuDefaultAndHiddenElements.ToArray());
				}
				else
				{
					log.Debug("UI - Etc clicked");
					etcMenu.Populate(etcMenuDefaultElements.ToArray());
				}

				etcMenu.Show(pEtc, DropdownPosition.BottomRight);
			}
		}

		private void HandlePreferenceClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Preference clicked");
				Raise(PreferenceClick);
			}
			else
			{
				log.DebugFormat("UI - Preference wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleQuitClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Quit clicked");
				TelemetryHelper.RecordFeature("Exit", "Clicked");
				Raise(QuitClick);
			}
			else
			{
				log.DebugFormat("UI - Quit wrongly clicked with {0}", e.Button);
			}
		}

		private void HandleRecentUrlChanged(object sender, EventArgs e)
		{
			SetUrlTooltip();
		}

		private void NavigateWeb()
		{
			webContextMenu.Hide();
			if (MainForm != null) MainForm.Hide();
			RecentUrlQuery.Instance.OpenLink();
		}

		private void HandleWebClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				log.Debug("UI - Web clicked");
				TelemetryHelper.RecordFeature("Web", "Navigate");
				NavigateWeb();
			}

			if (e.Button == MouseButtons.Right)
			{
				log.Debug("UI - Web dropdown shown");
				foreach (var element in webContextMenu.GetElements().Cast<DropdownCheckElement>())
				{
					element.Checked = element.Value == RecentUrlQuery.Instance.GetCurrentName();
				}

				webContextMenu.Show(pWeb, DropdownPosition.BottomRight);
			}
		}

		private void OpenJcAddress(string address)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				string url = "";
				try
				{
					string ticket = AuthenticationHelper.GetAuthTicket();
					url = string.Format(address, ticket);
					var sInfo = new ProcessStartInfo(url);
					Process.Start(sInfo);
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}

		private void RenderText()
		{
			label1.Clear()
				.AddWeightChange()
				.AddText(username)
				.AddColorChange()
				.AddText(GetUserIdText(userId))
				.RenderText();
		}

		private string GetUserIdText(string userId)
		{
			return userId != null ? string.Format(" ({0})", userId) : "";
		}

		private void SetUrlTooltip()
		{
			toolTip1.SetToolTip(pWeb, string.Format("{0} {1}", Labels.Menu_MainPage, RecentUrlQuery.Instance.GetCurrentName()));
		}

		private void PanelPaint(object sender, PaintEventArgs e)
		{
			if (!(sender is Panel panel)) return;
			// Draw background image even if in high contrast mode
			// See: https://stackoverflow.com/a/11110297/2295648
			e.Graphics.DrawImage(panel.BackgroundImage, (panel.Width - panel.BackgroundImage.Width) / 2, (panel.Height - panel.BackgroundImage.Height) / 2);
		}
	}
}