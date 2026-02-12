using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Menu
{
	public class MenuReportHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int FavoriteReportsFailedQueryInterval = 5 * 60 * 1000;
		private const int FavoriteReportsQueryInterval = 60 * 60 * 1000;
		private int? favoriteReportsLastQueryTick = null;
		private int? favoriteReportsLastFailedQueryTick = null;
		private const string FeatureName = "ReportsInMenu";

		private const int DisplayedReportsFailedQueryInterval = 5 * 60 * 1000;
#if DEBUG
		private const int DisplayedReportsQueryInterval = 1 * 60 * 1000;
#else
		private const int DisplayedReportsQueryInterval = 60 * 60 * 1000;
#endif
		private int? displayedReportsLastQueryTick = null;
		private int? displayedReportsLastFailedQueryTick = null;
		private bool shouldQueryReports = true;
		private object displayedReportsLock = new object();
		
		private DisplayedReports displayedReports;
		List<ConvertedImage> convertedImages = new List<ConvertedImage>();
		private int tableRowCount;
		private int[] rowHeights;
		private int imagesTotalHeight;

		public event EventHandler FeatureEnabled;
		public event EventHandler FeatureDisabled;

		public event EventHandler DisplayedReportQuerying;
		public event EventHandler DisplayedReportError;

		public DateTime? DisplayedReportsLastQueryTime;

		private FavoriteReport[] _favoriteReports;
		public FavoriteReport[] FavoriteReports
		{
			get { return _favoriteReports; }
			set
			{
				_favoriteReports = value;
				FavoriteReportsChanged?.Invoke(this, _favoriteReports);
			}
		}

		private bool _isFeatureEnabled;
		public bool IsFeatureEnabled
		{
			get
			{
				return false;
			}
			set
			{
				_isFeatureEnabled = value;
				if (_isFeatureEnabled) FeatureEnabled?.Invoke(this, EventArgs.Empty);
				else FeatureDisabled?.Invoke(this, EventArgs.Empty);
			}
		}

		public MenuReportHelper()
		{
			IsFeatureEnabled = FeatureSwitches.IsEnabled(FeatureName);
			FeatureSwitches.FeatureChanged += FeatureSwitches_FeatureChanged;
		}

		private void FeatureSwitches_FeatureChanged(object sender, SingleValueEventArgs<string> e)
		{
			if (!FeatureName.Equals(e.Value, StringComparison.Ordinal)) return;
			IsFeatureEnabled = FeatureSwitches.IsEnabled(FeatureName);
		}

		internal event EventHandler ImagesChanged;
		internal event EventHandler<IEnumerable<FavoriteReport>> FavoriteReportsChanged;

		internal void RefreshFavoriteReports()
		{
			if (!IsFeatureEnabled) return;
			try
			{
				int tick = Environment.TickCount;
				if (favoriteReportsLastQueryTick.HasValue && tick - favoriteReportsLastQueryTick < FavoriteReportsQueryInterval) return;
				if (favoriteReportsLastFailedQueryTick.HasValue && tick - favoriteReportsLastFailedQueryTick < FavoriteReportsFailedQueryInterval) return;
				var reports =
					ActivityRecorderClientWrapper.Execute(x => x.GetFavoriteReports(ConfigManager.UserId, Labels.Culture.Name));
				favoriteReportsLastQueryTick = Environment.TickCount;
				FavoriteReports = reports.ToArray();
			}
			catch (Exception ex)
			{
				log.Debug("Something went wrong when getting the favorite reports from the server.", ex);
				favoriteReportsLastFailedQueryTick = Environment.TickCount;
			}
		}

		internal void RefreshFavoriteReportsNoCheck()
		{
			try
			{
				var reports =
					ActivityRecorderClientWrapper.Execute(x => x.GetFavoriteReports(ConfigManager.UserId, Labels.Culture.Name));
				DisplayedReportsLastQueryTime = DateTime.Now;
				favoriteReportsLastQueryTick = Environment.TickCount;
				FavoriteReports = reports.ToArray();
			}
			catch (Exception ex)
			{
				log.Debug("Something went wrong when getting the favorite reports from the server.", ex);
				favoriteReportsLastFailedQueryTick = Environment.TickCount;
			}
		}

		internal Bitmap GetImage(int row, int column)
		{
			return convertedImages.First(x => x.Row == row && x.Column == column).Image;
		}

		internal int GetRowHeight(int fullHeight, int rowNum)
		{
			double ratio = (double)rowHeights[rowNum] / imagesTotalHeight;
			return (int)(fullHeight * ratio);
		}
		
		internal int GetImageWidth(int fullWidth, int row, int column)
		{
			int imagesFullWidth = 0;
			ConvertedImage currentImage = null;
			foreach (var convertedImage in convertedImages.Where(x => x.Row == row))
			{
				imagesFullWidth += convertedImage.Image.Width;
				if (convertedImage.Column == column) currentImage = convertedImage;
			}
			if (currentImage == null) return 0;
			double ratio = (double)fullWidth / imagesFullWidth;
			return (int)(currentImage.Image.Width * ratio);
		}

		internal int GetTableRowCount()
		{
			return tableRowCount;
		}

		internal int GetTableColumnCount(int row)
		{
			return convertedImages.Where(x => x.Row == row).Max(x => x.Column) + 1;
		}

		private void convertImages()
		{
			convertedImages.Clear();
			var xml = displayedReports.LayoutXml;
			XElement baseElement = XElement.Parse("<rows>" + xml + "</rows>");
			int i = 0;
			tableRowCount = baseElement.Elements("row").Count();
			rowHeights = new int[tableRowCount];
			imagesTotalHeight = 0;
			foreach (var row in baseElement.Elements("row"))
			{
				i++;
				int j = 0;
				int maxHeight = 0;
				foreach (var imgElement in row.Elements("img"))
				{
					j++;
					string id = imgElement.Attribute("id")?.Value;
					if (id == null) throw new Exception("img has no id value in xml");
					var displayedImage = displayedReports.ReportImages.First(x => x.Id == id);
					Bitmap bmp;
					if (displayedImage.ReportImage != null)
					{
						using (var ms = new MemoryStream(displayedImage.ReportImage))
						{
							bmp = new Bitmap(ms);
						}

						if (bmp.Height > maxHeight)
							maxHeight = bmp.Height;
					}
					else
					{
						bmp = new Bitmap(1, 1);
					}

					var image = new ConvertedImage()
					{
						Column = j - 1,
						Image = bmp,
						Row = i - 1
					};
					convertedImages.Add(image);
				}

				rowHeights[i - 1] = maxHeight;
				imagesTotalHeight += maxHeight;
			}
		}

		internal void RefreshDisplayedReports()
		{
			if (!IsFeatureEnabled) return;
			if (!shouldQueryReports) return;
			lock (displayedReportsLock)
			{
				try
				{
					//TODO: Check if nothing has changed
					int tick = Environment.TickCount;
					if (displayedReportsLastQueryTick.HasValue && tick - displayedReportsLastQueryTick < DisplayedReportsQueryInterval) return;
					if (displayedReportsLastFailedQueryTick.HasValue && tick - displayedReportsLastFailedQueryTick < DisplayedReportsFailedQueryInterval) return;
					DisplayedReportQuerying?.Invoke(this, EventArgs.Empty);
					displayedReports = ActivityRecorderClientWrapper.Execute(x => x.GetDisplayedReports(ConfigManager.UserId, Labels.Culture.Name));
					convertImages();
					DisplayedReportsLastQueryTime = DateTime.Now;
					displayedReportsLastQueryTick = Environment.TickCount;

					Platform.Factory.GetGuiSynchronizationContext().Send(_ =>
					{
						ImagesChanged?.Invoke(this, EventArgs.Empty);
					}, null);
				}
				catch (FaultException fex)
				{
					if (fex.Message == "Error occurred in PcServerMyPerformance (custom) report.")
					{
						shouldQueryReports = false;
						return;
					}
					displayedReportsLastFailedQueryTick = Environment.TickCount;
					DisplayedReportError?.Invoke(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					displayedReportsLastFailedQueryTick = Environment.TickCount;
					log.Warn("Couldn't get displayed reports.", ex);
					DisplayedReportError?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		internal void ResetLastQueryTime()
		{
			displayedReportsLastQueryTick = null;
			favoriteReportsLastQueryTick = null;
		}

		class ConvertedImage
		{
			public Bitmap Image { get; set; }
			public int Row { get; set; }
			public int Column { get; set; }
		}
	}

}
