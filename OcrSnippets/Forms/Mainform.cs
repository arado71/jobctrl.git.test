using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ocr.Engine;
using Ocr.Learning;
using Ocr.Model;
using Ocr.Recognition;
using OcrConfig;
using OcrConfig.Forms;
using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;
using Tct.ActivityRecorderService.Ocr;
using TcT.ActivityRecorderClient.Capturing.Plugins.Ocr;
using TcT.ActivityRecorderClient.SnippingTool;

namespace TcT.OcrSnippets
{
	public partial class MainForm : Form
	{
		private class ContributionItems : List<ContributionItem>
		{
			public event EventHandler OnCleared;
			public event EventHandler OnChanged;
			public bool IsFirstTime = true;

			public void Update(IEnumerable<ContributionItem> newItems)
			{
				foreach (var item in newItems)
				{
					var i = this.FirstOrDefault(e => e.Guid == item.Guid);
					if (i == null) continue;
					i.Content = item.Content;
					i.WindowTitle = item.WindowTitle;
					if (!IsFirstTime)
						OnChanged?.Invoke(this, EventArgs.Empty);
				}
			}
			public new void Clear()
			{
				base.Clear();
				OnCleared?.Invoke(this, EventArgs.Empty);
			}
		}

		private bool capturing;
		private bool isSnipping, isSnippetsOpen, isProcessingActive;
		private IntPtr clipboardViewerNext;
		private readonly List<Hotkey> Hotkeys = new List<Hotkey>();
		private ContributionItems contributionItems;
		private IEnumerable<ContributionItem> DistinctAreas
		{
			get
			{
				return (from e in contributionItems
						group e by new { e.Area }
						into areaGroup
						select areaGroup.FirstOrDefault()).ToList();
			}
		}
		public MainForm()
		{
			InitializeComponent();
			SnippingTool.AreaSelected += SnippingToolOnAreaSelected;
			SnippingTool.Cancel += SnippingToolOnCancel;
			// Recognition.ImageChanged += (_, args) => { Imager.Display(args.Bitmap, args.Title); };
			ContributionForm.FormClosing += (o, __) =>
			{
				var sender = o as ContributionForm;
				if (sender.DialogResult != DialogResult.OK) return;
				contributionItems.Update(sender.Resolutions);
				if (contributionItems.Count == contributionItems.Count(e => !string.IsNullOrEmpty(e.Content)))
				{
					mnuProcessSnippets.Enabled = true;
					ShowBaloonMessage("Results are ready for learning...");
				}
				else
					ShowBaloonMessage("There were no/less content added...");
			};
			TrainingOptimizationForm.OnFinished += (_, e) => processMoreResults(e);
			TransformConfigurationExt.Swipe();
			WindowState = FormWindowState.Minimized;
			notifyIcon.Visible = true;
			contributionItems = new ContributionItems();
			contributionItems.OnCleared += (_, __) =>
			{
				contributionItems.IsFirstTime = true;
				mnuShowSnippets.Enabled =
				mnuProcessSnippets.Enabled =
				mnuProcessAgain.Enabled = false;
				mnuProcessSnippets.Text = "Process Snippets";
			};
			contributionItems.OnChanged += (_, __) =>
			{
				mnuProcessSnippets.Text = "ReProcess Snippets";
				mnuProcessAgain.Enabled = true;
			};
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			mnuLanguageCombo.Items.AddRange(GetLanguageNames());
			mnuLanguageCombo.SelectedItem = "eng";
			mnuCharSetCombo.Items.AddRange(LearningHelper.GetCharSets());
			mnuCharSetCombo.SelectedItem = LearningHelper.OcrCharSets.Numbers.ToString();
			mnuContributionModeCombo.Items.AddRange(Enum.GetNames(typeof(ContributionModeEnum)));
			mnuContributionModeCombo.SelectedItem = ContributionModeEnum.Distributed.ToString();
			mnuIgnoreCaseCombo.Items.AddRange(new object[] {
				"Disabled", "Enabled"
			});
			mnuIgnoreCaseCombo.SelectedIndex = 0;
			RegisterClipboardViewer();
			RegisterHotKey(Keys.C, Hk_Win_C_OnPressed);
			RegisterHotKey(Keys.R, Hk_Win_R_OnPressed);
			ShowBaloonMessage("Double-click the systray icon or press CTRL+WIN+C to start a new snip...");
			Hide();
		}
		private void Form1_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				this.Hide();
				notifyIcon.Visible = true;
			}
		}
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
			if (notifyIcon != null)
				notifyIcon.Visible = true;
		}
		private object[] GetLanguageNames()
		{
			List<object> languages = new List<object>();
			languages.Clear();
			foreach (var f in Directory.EnumerateFiles(TesseractEngineEx.TessDataPath, "*.traineddata"))
			{
				var fi = new FileInfo(f);
				var name = Path.GetFileNameWithoutExtension(fi.Name);
				if (languages.All(item => (string)item != name))
					languages.Add(name);
			}
			return languages.ToArray();
		}
		private void RegisterClipboardViewer()
		{
			clipboardViewerNext = User32.SetClipboardViewer(this.Handle);
		}
		private void UnregisterClipboardViewer()
		{
			User32.ChangeClipboardChain(this.Handle, clipboardViewerNext);
		}
		private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			StartSnipping();
		}
		private void RegisterHotKey(Keys character, HandledEventHandler handler)
		{
			var hk = new Hotkey { KeyCode = character, Windows = true, Control = true };
			hk.Pressed += handler;

			if (!hk.GetCanRegister(this))
				Console.WriteLine("Already registered");
			else
				hk.Register(this);
			Hotkeys.Add(hk);
		}
		private void Hk_Win_C_OnPressed(object sender, HandledEventArgs handledEventArgs)
		{
			StartSnipping();
		}
		private void Hk_Win_R_OnPressed(object sender, HandledEventArgs handledEventArgs)
		{
			Recording();
		}
		private void Recording()
		{
			if (!DistinctAreas.Any())
			{
				ShowBaloonMessage("Recording not allowed until at least one area isn't selected");
				return;
			}
			foreach (var area in DistinctAreas)
				contributionItems.Add(new ContributionItem
				{
					Image = ScaleImage(area.AppWindow.Handle, area.AppWindow.WindowRect, area.Area, area.Monitor),
					Area = area.Area,
					ProcessName = area.ProcessName,
					WindowTitle = area.WindowTitle,
					Monitor = area.Monitor,
					AppWindow = area.AppWindow
				});
			ShowBaloonMessage("Snapshot taken");
		}

		private void UnregisterHotkey()
		{
			foreach (var hotkey in Hotkeys)
				if (hotkey.Registered)
					hotkey.Unregister();
		}
		protected override void WndProc(ref Message m)
		{
			switch ((Msgs)m.Msg)
			{
				case Msgs.WM_DRAWCLIPBOARD:
					OnClipboardData();
					User32.SendMessage(clipboardViewerNext, m.Msg, m.WParam, m.LParam);
					break;
				case Msgs.WM_CHANGECBCHAIN:
					Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");
					if (m.WParam == clipboardViewerNext)
					{
						clipboardViewerNext = m.LParam;
					}
					else
					{
						User32.SendMessage(clipboardViewerNext, m.Msg, m.WParam, m.LParam);
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}
		// Called when there is new data in clipboard
		private void OnClipboardData()
		{
			if (capturing)
				ProcessClipboardData();
		}
		private void ProcessClipboardData()
		{
			ShowBaloonMessage("Processing clipboard image...");
			int retries = 2;
			while (retries-- > 0)
			{
				var img = Clipboard.GetImage();
				if (img != null)
				{
					ProcessOcrImage(img);
					break;
				}
				Thread.Sleep(100);
			}
		}
		private void ProcessOcrImage(Image image)
		{
			var lang = mnuLanguageCombo.SelectedItem.ToString();
			var result = Ocr.Process(image, lang);
			notifyIcon.Visible = true; // hide balloon tip (if any)
			OcrResultForm.ShowOcr(result);
		}
		private void mnuExit_Click(object sender, EventArgs e)
		{
			notifyIcon.Visible = false;
			notifyIcon = null;
			UnregisterClipboardViewer();
			UnregisterHotkey();
			Application.Exit();
			Environment.Exit(0);
		}
		private void mnuMonitorClipboard_Click(object sender, EventArgs e)
		{
			mnuMonitorClipboard.Checked = !mnuMonitorClipboard.Checked;
			if (mnuMonitorClipboard.Checked)
			{
				capturing = true;
				OnClipboardData();
			}
			else
				capturing = false;
		}
		private void ShowBaloonMessage(string text)
		{
			notifyIcon.BalloonTipText = text;
			notifyIcon.BalloonTipTitle = "OCR Snippets";
			notifyIcon.ShowBalloonTip(1000);
		}
		private void ShowBaloonMessage(string text, params object[] args)
		{
			var stringFormat = string.Format(text, args);
			ShowBaloonMessage(stringFormat);
		}
		private void mnuClipboardNow_Click(object sender, EventArgs e)
		{
			if (!Clipboard.ContainsImage()) return;
			ProcessClipboardData();
		}
		private void mnuSnip_Click(object sender, EventArgs e)
		{
			StartSnipping();
		}
		private void StartSnipping()
		{
			if (!isSnipping)
			{
				isSnipping = true;
				SnippingTool.Snip();
			}
		}
		private void SnippingToolOnCancel(object sender, EventArgs e)
		{
			isSnipping = false;
		}
		private void SnippingToolOnAreaSelected(object sender, SelectedEventArgs sea)
		{
			isSnipping = false;
			EnumWindowsHelper.WindowInfo wi;
			var image = GetScaledImage(sea, out wi);
			if (image == null)
			{
				ShowBaloonMessage("Capturing failed! Select the destination window prior to snipping!");
				return;
			}
			if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				ShowBaloonMessage("Processing image...");
				ProcessOcrImage(image);
			}
			else
			{
				Clipboard.SetImage(image);
				Rectangle iarea = new Rectangle(sea.Rectangle.X - wi.ClientRect.X, sea.Rectangle.Y - wi.ClientRect.Y, sea.Rectangle.Width, sea.Rectangle.Height);
				Persist(wi, image, new SelectedEventArgs
				{
					Rectangle = iarea,
					Monitor = sea.Monitor
				});
				mnuClipboardNow.Enabled = true;
				mnuMonitorClipboard.Enabled = true;
				mnuShowSnippets.Enabled = true;
				ShowBaloonMessage("Image persisted and copied into Clipboard");
			}
		}
		private void mnuShowSnippets_Click(object sender, EventArgs e)
		{
			if (!isSnippetsOpen)
				ShowSnippets();
		}
		private void ShowSnippets()
		{
			isSnippetsOpen = true;
			ContributionForm form = new ContributionForm(contributionItems);
			DialogResult res = form.ShowDialog(this);
			if (res == DialogResult.Abort) // 'clear' action
				contributionItems.Clear();
			isSnippetsOpen = false;
		}
		private void mnuProcessSnippets_Click(object sender, EventArgs ea)
		{
			if (isProcessingActive) return;
			isProcessingActive = true;
			ProcessSnippets(false);
			contributionItems.IsFirstTime = false;
			isProcessingActive = false;
		}
		private void mnuProcessAgain_Click(object sender, EventArgs e)
		{
			ProcessSnippets(true);
		}
		private void ProcessSnippets(bool reProcess)
		{
			// create samples for PSO
			SampleStorage samples = new SampleStorage();
			foreach (var item in contributionItems)
				if (!string.IsNullOrEmpty(item.Content))
					samples.Set(new Bitmap(item.Image), item.Content, 0);
			if (samples.Count == 0) return;
			var config = new OcrConfiguration
			{
				Language = mnuLanguageCombo.SelectedItem.ToString(),
				DestinationLanguageFile = mnuLanguageCombo.SelectedItem.ToString(),
				CharSet = (LearningHelper.OcrCharSets)mnuCharSetCombo.SelectedIndex,
				UserContribution = (ContributionModeEnum)mnuContributionModeCombo.SelectedIndex == ContributionModeEnum.Distributed,
				ContentRegex = mnuContentRegexTextbox.Text,
				IgnoreCase = mnuIgnoreCaseCombo.SelectedItem.ToString().Equals("Enabled"),
				HorizontalAlign = HorizontalAlign.Stretch,
				VerticalAlign = VerticalAlign.Stretch
			};
			MathNet.Numerics.LinearAlgebra.Double.DenseVector denseVector = null;
			if (TransformConfigurationExt.HasResults)
				denseVector = TransformConfigurationExt.Load();
			if (reProcess)
			{
				Stopwatch sw = Stopwatch.StartNew();
				var diff = TrainingOptimizationForm.RefineMetric(denseVector, samples, config);
				var elapsed = sw.ElapsedMilliseconds;
				if (denseVector != null)
					ShowBaloonMessage("Refine processing complete. Diff was {0} and {1} ms taken", diff, elapsed);
			}
			else
			{
				TrainingOptimizationForm tf = new TrainingOptimizationForm(
					samples,
					config);
				tf.ShowDialog();
			}
		}
		private void processMoreResults(TrainingResultEventArgs e)
		{
			OcrConfiguration result = e.Config;
			StringBuilder sb = new StringBuilder();
			int keyNum = 1;
			foreach (var res in DistinctAreas)
			{
				OcrConfiguration oc = (OcrConfiguration)result.Clone();
				oc.Area = res.Area;
				oc.ProcessName = res.ProcessName;
				oc.WindowTitle = res.WindowTitle;
				sb.Append(sb.Length == 0
					? string.Format("// {0} areas were found ({1})\r\n", DistinctAreas.Count(), DateTime.Now.ToLongTimeString())
					: ";" + Environment.NewLine)
					.Append("myKey")
					.Append(keyNum++)
					.Append("=")
					.Append(oc);
			}
			sb.Append("// One line result to copy:");
			Clipboard.SetText(sb.ToString());
			ShowBaloonMessage("Key results are ready and placed onto Clipboard...");
		}

		private Image ScaleImage(IntPtr appWindowHandle, Rectangle appWindowWindowRect, Rectangle selectionRectangle, DeviceInfo selectionMonitor)
		{
			// app window rect
			Rectangle appWindowRectScaled = EnumWindowsHelper.CorrectCoordinates(appWindowHandle, appWindowWindowRect);
			Rectangle rectFromWin32 = EnumWindowsHelper.GetWindowRect(appWindowHandle);
			Rectangle rect = EnumWindowsHelper.CorrectCoordinates(appWindowHandle, rectFromWin32);
			var X = (int)(rect.Left * selectionMonitor.HScale);
			var Y = (int)(rect.Top * selectionMonitor.VScale);
			var scaledRect = new Rectangle(
				X, Y,
				(int)(rect.Right * selectionMonitor.HScale) - X,
				(int)(rect.Bottom * selectionMonitor.VScale) - Y);
			// app window full capture
			var bmp = new Bitmap(scaledRect.Width, scaledRect.Height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bmp);
			graphics.CopyFromScreen(scaledRect.Left, scaledRect.Top, 0, 0, new Size(scaledRect.Width, scaledRect.Height), CopyPixelOperation.SourceCopy);
			// scaled rect over the capture
			var area = new Rectangle(
				(int)(selectionRectangle.Left * selectionMonitor.HScale) - (int)(appWindowRectScaled.Left * selectionMonitor.HScale) + selectionMonitor.Screen.Bounds.Left,
				(int)(selectionRectangle.Top * selectionMonitor.HScale) - (int)(appWindowRectScaled.Top * selectionMonitor.VScale),
				(int)(selectionRectangle.Width * selectionMonitor.HScale),
				(int)(selectionRectangle.Height * selectionMonitor.VScale));
			// sliced part of capture
			var image = new Bitmap(area.Width, area.Height);
			using (Graphics gr = Graphics.FromImage(image))
			{
				gr.DrawImage(bmp,
					new Rectangle(0, 0, image.Width, image.Height),
					area,
					GraphicsUnit.Pixel);
			}
			return image;
		}

		private Image GetScaledImage(SelectedEventArgs selection, out EnumWindowsHelper.WindowInfo appWindow)
		{
			// figure out active app window
			var cPos = Cursor.Position;
			var windows = EnumWindowsHelper.GetWindowsInfo(Stopwatch.StartNew());
			var cursorWindows = windows.Where(window => window.WindowRect.Contains(cPos));  // windows under the mouse 
			var fw = EnumWindowsHelper.GetForegroundWindow();
			var areaWindows = cursorWindows.Where(x => x.Handle == fw);                     // windows on the asked display
			appWindow = areaWindows.FirstOrDefault();
			if (appWindow == null) return null;
			return ScaleImage(
				appWindow.Handle,
				appWindow.WindowRect,
				selection.Rectangle,
				selection.Monitor);
		}
		private void Persist(EnumWindowsHelper.WindowInfo appWindow, Image image, SelectedEventArgs selection)
		{
			try
			{
				contributionItems.Add(new ContributionItem
				{
					Image = image,
					Area = selection.Rectangle,     // original selection instead of cropped one
					Monitor = selection.Monitor,
					AppWindow = appWindow,
					ProcessName = appWindow.ProcessName,
					WindowTitle = appWindow.Title
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Persistence failed ({ex.Message})");
			}
		}
		private enum ContributionModeEnum
		{
			Distributed,
			Central
		}
	}
}
