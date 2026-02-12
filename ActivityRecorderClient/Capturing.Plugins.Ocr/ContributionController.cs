using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.Controller;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public class ContributionController
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly object lockObject = new object();
		private static volatile ContributionController instance;
		private const int FILES_PERSISTED_DAYS_COUNT = 30;
		private const int DELETE_ALL_FILES = 0;
		private readonly TimeSpan SAMPLE_COUNT_REFRESH_INTERVAL = TimeSpan.FromMinutes(10);
		private readonly SnippingDataUploadManager snippingDataUploadManager = new SnippingDataUploadManager();
		private readonly LastInputInfo lastInputInfo = new LastInputInfo();
		private DateTime lastRun = DateTime.MinValue;
		private DateTime firstWorkingOfTheDay = DateTime.MinValue;
		private CurrentWorkController currentWorkController;
		private DateTime sampleCountLastRefresh;
		private int sampleCount;
		private ContributionController()
		{
			ContributionForm.OnRemoveItem += (object o, SingleValueEventArgs<string> e) =>
			{
				IsolatedStorageHelper.Delete(e.Value);
			};

			ContributionForm.FormClosing += (o, __) =>
			{
				var sender = o as ContributionForm;
				if (sender == null) return;
				lastRun = DateTime.Now;
				formIsShown = false;
				var items = sender.Resolutions;
				switch (sender.DialogResult)
				{
					case DialogResult.OK:
						PersistContentedSnippets(items);
						break;

					case DialogResult.Abort:
						DeleteContentedSnippets(items);
						break;
				}
			};

			snippingDataUploadManager.Start();
		}
		public static ContributionController Instance
		{
			get
			{
				if (instance == null)
					lock (lockObject)
					{
						if (instance == null)
							instance = new ContributionController();
					}
				return instance;
			}
		}
		private void PersistContentedSnippets(IEnumerable<Snippet> d)
		{
			foreach (var snippet in d.Where(e => !string.IsNullOrEmpty(e.Content)))
				if (IsolatedStorageHelper.Save(snippet))
					IsolatedStorageHelper.Delete(snippet.ImageFileName);
		}

		private void DeleteContentedSnippets(IEnumerable<Snippet> snippets)
		{
			foreach (var snippet in snippets)
				IsolatedStorageHelper.Delete(snippet.ImageFileName);
		}

		public void Start(PluginOcrModeEnum mode)
		{
			ThreadPool.QueueUserWorkItem(_ =>
				IsolatedStorageHelper.SwipeOutdatedFiles(mode == PluginOcrModeEnum.Offline ? DELETE_ALL_FILES : -FILES_PERSISTED_DAYS_COUNT)
				, null
			);
		}
		public void PersistImage(Bitmap windowImage, int ruleId, string processName, string contentGuess)
		{
			IsolatedStorageHelper.Save(windowImage, ruleId, processName, contentGuess);
		}

		public void PersistImageReadyToUpload(Bitmap windowImage, int ruleId, string processName)
		{
			var snippet = new Snippet
			{
				Image = windowImage,
				Guid = Guid.NewGuid(),
				UserId = ConfigManager.UserId,
				RuleId = ruleId,
				ProcessName = processName,
				Content = ""
			};
			IsolatedStorageHelper.Save(snippet);
		}

		private List<Snippet> CollectData()
		{
			List<Snippet> ret = new List<Snippet>();
			var images = IsolatedStorageHelper.GetImages.Take(10);
			foreach (var b in images)
				ret.Add(new Snippet
				{
					Image = b.Image,
					Guid = Guid.NewGuid(),
					UserId = ConfigManager.UserId,
					RuleId = b.RuleId,
					ProcessName = b.ProcessName,
					ImageFileName = b.FileName, 
					Content = b.ContentGuess
				});
			return ret;
		}

		private bool formIsShown = false;
		internal void PopupContributionIfNeeded()
		{
			const int AT_LEAST_NUMBER_OF_SAMPLES_NEEDED = 1;

#if (DEBUG || DEV)
			const int CONTRIBUTION_FORM_DELAY_MINS = 1;               // after 1 minute
#else
			const int CONTRIBUTION_FORM_DELAY_MINS =  60;		// after one hour
#endif

			if (IsNotToday(firstWorkingOfTheDay)) return;
			if (lastRun > DateTime.MinValue && !IsNotToday(lastRun)) return;
			if ((DateTime.Now - firstWorkingOfTheDay).TotalMinutes < CONTRIBUTION_FORM_DELAY_MINS) return;
			if (GetSampleCount() < AT_LEAST_NUMBER_OF_SAMPLES_NEEDED) return;
			PopupContributionForm(false);
		}

		internal void PopupContributionForm(bool fromMenu = false)
		{
			lock (lockObject)
			{
				if (formIsShown && !fromMenu) return;
				if (lastRun > DateTime.MinValue && !IsNotToday(lastRun) && !fromMenu) return;
				try
				{
					ImageStorageCleaner.CleanStoredImages();
					var snippets = CollectData();
					if (snippets.Count == 0)
					{
						log.Debug("OCR no image to be able to content");
						return;
					}
					var guiContext = Platform.Factory.GetGuiSynchronizationContext();
					if (guiContext == null) return;
					guiContext.Post(_ =>
					{
						var form = new ContributionForm(snippets);
						form.Show();
					}, null);
					formIsShown = true;
				}
				catch (Exception ex)
				{
					log.Error("ContributionForm not shown error:" + ex.Message);
				}
			}
		}

		private int GetSampleCount()
		{
			if ((DateTime.Now - sampleCountLastRefresh) > SAMPLE_COUNT_REFRESH_INTERVAL)
			{
				sampleCount = IsolatedStorageHelper.GetImages.Count();
				sampleCountLastRefresh = DateTime.Now;
				log.DebugFormat("Sample count updated to {0}", sampleCount);
			}
			return sampleCount;
		}

		public void AddCurrentWorkController(CurrentWorkController controller)
		{
			currentWorkController = controller;
			currentWorkController.PropertyChanged += CurrentWorkChanged;
		}

		private void CurrentWorkChanged(object sender, PropertyChangedEventArgs pcea)
		{
			if (pcea.PropertyName == "CurrentWorkState" && currentWorkController.IsWorking)
			{
				if (IsNotToday(firstWorkingOfTheDay))
				{
					firstWorkingOfTheDay = DateTime.Now;
					log.Debug("firstWorkingOfTheDay is updated " + firstWorkingOfTheDay);
				}
			}
		}

		private bool IsNotToday(DateTime otherDate)
		{
			return (otherDate.DayOfYear != DateTime.Now.DayOfYear || otherDate.Year != DateTime.Now.Year);
		}
	}
}
