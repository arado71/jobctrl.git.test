using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Screenshots;
using Tct.ActivityRecorderClient.View;
using Timer = System.Threading.Timer;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginScreenAnalyst : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static int instanceCounter;
		private static FullScreenBorderAlertForm alertForm;
		private static Form layerForm;
		private static int lastResult = -1;
		private static bool isLayerFormVisible;

		public const string PluginId = "JobCTRL.ScreenAnalyst";
		public string Id => PluginId;
		public const string KeyIsProductive = "IsProductive";
		public const string KeyProductiveTime = "ProductiveTime";
		public const string KeyNonProductiveTime = "NonProductiveTime";
		public const string ValueUnknown = "unknown";
		private const string ParamMode = "Mode";

		private bool isInitialized;
		private Timer activeTimer;
		private ActivityRecorderForm mainForm;
		private bool isDebugMode;
		private Point previewCursorPosition;
		private ScreenshotAnalyzerSettingsForm settingsForm;

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamMode;	
		}

		public void SetParameter(string name, string value)
		{
			if (name.Equals(ParamMode))
				isDebugMode = value.Equals("debug", StringComparison.InvariantCultureIgnoreCase);
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyIsProductive;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!isInitialized)
			{
				if (!TryInitialize())
				{
					yield return new KeyValuePair<string, string>(KeyIsProductive, ValueUnknown);
					yield break;
				}

				isInitialized = true;
			}

			CheckActive();
			if (!ScreenshotAnalystManager.IsScreenshotAnalyzerEnabled) yield break;
			var result = ScreenshotAnalystManager.IsPictureDetected;
			var intResult =  result.HasValue ? result.Value ? 1 : 0 : -1;
			var resultChanged = intResult != Interlocked.Exchange(ref lastResult, intResult );
			if (isDebugMode && resultChanged)
			{
				mainForm.GuiContext.Post(_ =>
				{
					if (mainForm?.niStatusIcon == null) return; // during dispose
					if (result.HasValue)
					{
						alertForm.BorderColor = result.Value ? Color.OrangeRed : Color.Aqua;
						alertForm.ShowOnce(1000);
					}
					mainForm.niStatusIcon.Icon = result.HasValue ? result.Value ? Resources.Button_Blank_Red : Resources.Button_Blank_Blue : Resources.Button_Blank_Gray;
					SetTooltipText(result);
				}, null);
			}
			yield return new KeyValuePair<string, string>(KeyIsProductive, intResult.ToString());
			var nonProductiveTime = (int)((ScreenshotAnalystManager.NonProductiveTime + 30000L) / 60000L);
			var productiveTime = (int)((ScreenshotAnalystManager.ProductiveTime + 30000L) / 60000L);
			yield return new KeyValuePair<string, string>(KeyNonProductiveTime, nonProductiveTime.ToString());
			yield return new KeyValuePair<string, string>(KeyProductiveTime, productiveTime.ToString());
		}

		private void SetTooltipText(bool? result)
		{
			var totalTime = ScreenshotAnalystManager.ProductiveTime + ScreenshotAnalystManager.NonProductiveTime;
			var ratio = totalTime > 0L ? ScreenshotAnalystManager.ProductiveTime * 100L / totalTime : 0L;
			var statText = $"{TimeSpan.FromMilliseconds(ScreenshotAnalystManager.ProductiveTime):hh\\:mm}/{TimeSpan.FromMilliseconds(totalTime):hh\\:mm} {ratio:D}%";
			if (mainForm?.niStatusIcon != null) mainForm.niStatusIcon.Text = statText + "\n\n" + (result.HasValue ? result.Value ? "Picture detected on the screen" : "No picture" : "Inactive");
			if (settingsForm != null) settingsForm.WorkTimeStatText = statText;
		}

		private bool TryInitialize()
		{
			var winPlatform = Platform.Factory as Platform.PlatformWinFactory;
			if (winPlatform?.MainForm == null) return false;
			mainForm = winPlatform.MainForm;

			return true;
		}

		private void CheckActive()
		{
			if (activeTimer == null)
			{
				activeTimer = new Timer(ActiveTimerExceed, null, Timeout.Infinite, Timeout.Infinite);
				if (Interlocked.Increment(ref instanceCounter) == 1)
				{
					ScreenshotAnalystManager.IsScreenshotAnalyzerEnabled = true;
					if (isDebugMode)
					{
						mainForm.GuiContext.Post(_ =>
						{
							alertForm = new FullScreenBorderAlertForm();
							alertForm.Owner = mainForm;
							alertForm.BorderSize = 5;
							mainForm.niStatusIcon.MouseMove += StatusIconMouseMove;
						}, null);
					}
				}
				if (isDebugMode)
				{
					mainForm.GuiContext.Post(_ =>
					{
						if (mainForm?.niStatusIcon == null) return; // during dispose
						if (!mainForm.niStatusIcon.Visible)
						{
							mainForm.niStatusIcon.Visible = true;
							log.Debug("Status icon visible");
						}
					}, null);
				}
			}

			activeTimer.Change(15000, Timeout.Infinite);
		}

		private void StatusIconMouseMove(object sender, MouseEventArgs e)
		{
			if (Screen.PrimaryScreen.WorkingArea.Contains(Cursor.Position) || instanceCounter <= 0) return;
			SetTooltipText(ScreenshotAnalystManager.IsPictureDetected);
			previewCursorPosition = Cursor.Position;
			ScreenshotAnalystManager.StartPreview(StartPreviewCallback);
		}

		private void StartPreviewCallback()
		{
			mainForm.GuiContext.Post(_ =>
			{
				if (instanceCounter <= 0) return;
				if (layerForm == null && ScreenshotAnalystManager.PreviewImages != null)
				{
					layerForm = new FormWithoutActivation { FormBorderStyle = FormBorderStyle.None, WindowState = FormWindowState.Maximized, TopMost = true, BackgroundImageLayout = ImageLayout.None, BackgroundImage = ScreenshotAnalystManager.PreviewImages[0], ShowInTaskbar = false };
					layerForm.Owner = mainForm;
					layerForm.MouseMove += LayerFormMouseMove;
					layerForm.VisibleChanged += LayerFormVisibleChanged;
				}

				if (layerForm != null && !layerForm.Visible && !layerForm.IsDisposed)
				{
					layerForm.BackgroundImage = ScreenshotAnalystManager.PreviewImages[0];
					layerForm.Visible = true;
#if DEBUG
					layerForm.TopMost = false;
#endif
				}
				if (settingsForm == null)
				{
					settingsForm = new ScreenshotAnalyzerSettingsForm();
					settingsForm.Top = Screen.PrimaryScreen.WorkingArea.Bottom - settingsForm.Height;
					settingsForm.Left = Screen.PrimaryScreen.WorkingArea.Right - settingsForm.Width * 3 / 2;
					settingsForm.SlidingChanged += SettingsFormSlidingChanged;
					settingsForm.Owner = mainForm;
					settingsForm.Configs = ScreenshotAnalystManager.Configs;
					SetTooltipText(ScreenshotAnalystManager.IsPictureDetected);
				}
				if (settingsForm.Visible)
					settingsForm.Visible = false;
				settingsForm.Visible = true;
			}, null);
		}

		private void SettingsFormSlidingChanged(object sender, bool e)
		{
			if (e || instanceCounter <= 0)
				return;

			layerForm.Visible = false;
			settingsForm.Visible = false;

			ScreenshotAnalystManager.UpdateConfigs();
			ScreenshotAnalystManager.StartPreview(StartPreviewCallback);
		}

		private void LayerFormMouseMove(object sender, MouseEventArgs e)
		{
			if (instanceCounter <= 0) return;
			if (Math.Abs(e.X - previewCursorPosition.X) <= 10 && Math.Abs(e.Y - previewCursorPosition.Y) <= 10) return;
			var rect = settingsForm.RectangleToScreen(settingsForm.DisplayRectangle);
			if (settingsForm?.Visible == true && (rect.Contains(e.Location) || e.Y < previewCursorPosition.Y && e.Y > rect.Bottom && Math.Abs(e.X - previewCursorPosition.X) <= 50)) return;
			layerForm.Visible = false;
			settingsForm?.Hide();
		}

		private void LayerFormVisibleChanged(object sender, EventArgs e)
		{
			if (layerForm.Visible || instanceCounter <= 0) return;
			ScreenshotAnalystManager.StopPreview();
		}

		private void ActiveTimerExceed(object state)
		{
			if (Interlocked.Decrement(ref instanceCounter) <= 0)
			{
				ScreenshotAnalystManager.IsScreenshotAnalyzerEnabled = false;
				mainForm.GuiContext.Post(_ =>
				{
					if (mainForm.niStatusIcon.Visible)
					{
						mainForm.niStatusIcon.Visible = false;
						log.Debug("Status icon invisible");
					}
					mainForm.niStatusIcon.MouseMove -= StatusIconMouseMove;

					alertForm.Close();
					alertForm.Dispose();
					alertForm = null;

					if (layerForm != null) {
						layerForm.MouseMove -= LayerFormMouseMove;
						layerForm.VisibleChanged -= LayerFormVisibleChanged;
						layerForm.Dispose();
						layerForm = null;
					}

					if (settingsForm != null)
					{
						settingsForm.SlidingChanged -= SettingsFormSlidingChanged;
						settingsForm.Dispose();
						settingsForm = null;
					}
				}, null);
			}
			activeTimer.Dispose();
			activeTimer = null;
		}
	}

}
