using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using System.Xml.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Menu
{
	public class MenuTabHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int DisplayedTabsFailedQueryInterval = 5 * 60 * 1000;
#if DEBUG
		private const int DisplayedTabsQueryInterval = 1 * 60 * 1000;
#else
		private const int DisplayedTabsQueryInterval = 60 * 60 * 1000;
#endif
		private int? displayedTabsLastQueryTick = null;
		private int? displayedTabsLastFailedQueryTick = null;
		private List<ClientTab> displayedTabNames = new List<ClientTab>();
		private CachedDictionary<string, List<MenuTabRow>> tabCache = new CachedDictionary<string, List<MenuTabRow>>(TimeSpan.FromMinutes(15), true);

		public event EventHandler DisplayedTabError;
		public event EventHandler<List<ClientTab>> TabsChanged;
		public DateTime? DisplayedTabsLastQueryTime;
		private object displayedTabsLock = new object();

		private Dictionary<string, Color> colorDictionary = new Dictionary<string, Color>
		{
			{"blue", MetroFramework.MetroColors.Blue },
			{"red", MetroFramework.MetroColors.Red },
		};

		public MenuTabHelper()
		{
		}

		internal List<ClientTab> GetTabNames()
		{
			return displayedTabNames.ToList();
		}

		internal void RefreshTabNames()
		{
			lock (displayedTabsLock)
			{
				try
				{
					//TODO: Check if nothing has changed
					int tick = Environment.TickCount;
					if (displayedTabsLastQueryTick.HasValue && tick - displayedTabsLastQueryTick < DisplayedTabsQueryInterval) return;
					if (displayedTabsLastFailedQueryTick.HasValue && tick - displayedTabsLastFailedQueryTick < DisplayedTabsFailedQueryInterval) return;
					var oldTabNames = displayedTabNames.ToList();
					displayedTabNames = ActivityRecorderClientWrapper.Execute(x => x.GetCustomTabs(ConfigManager.UserId, Labels.Culture.Name));
					DisplayedTabsLastQueryTime = DateTime.Now;
					displayedTabsLastQueryTick = Environment.TickCount;

					Platform.Factory.GetGuiSynchronizationContext().Send(_ =>
					{
						TabsChanged?.Invoke(this, oldTabNames);
					}, null);
				}
				catch (FaultException fex)
				{
					log.Warn("Couldn't ged tabs.", fex);
					displayedTabsLastFailedQueryTick = Environment.TickCount;
					DisplayedTabError?.Invoke(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					displayedTabsLastFailedQueryTick = Environment.TickCount;
					log.Warn("Couldn't get tabs.", ex);
					DisplayedTabError?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		internal void RequestTabContent(string tabId, Action<List<MenuTabRow>> callBack, Action onError)
		{
			try
			{
				List<MenuTabRow> tabRowList;
				if (tabCache.TryGetValue(tabId, out tabRowList))
				{
					callBack(tabRowList); return;
				}
				var content = ActivityRecorderClientWrapper.Execute(x => x.GetDisplayedReportForTabId(ConfigManager.UserId, Labels.Culture.Name, tabId, null));
				tabRowList = ConvertReport(content);
				tabCache.Add(tabId, tabRowList);
				callBack(tabRowList);
			}
			catch (Exception ex)
			{
				log.Warn("Unexpected error in requesting tab content.", ex);
				onError();
			}
		}

		internal void ResetLastQueryTime()
		{
			displayedTabsLastQueryTick = null;
		}

		private List<MenuTabRow> ConvertReport(DisplayedReports reports)
		{
			var xml = reports.LayoutXml;
			log.Debug(xml);
			XElement baseElement = XElement.Parse(xml);
			int i = 0;
			var result = new List<MenuTabRow>();
			int tableRowCount = baseElement.Elements("row").Count();
			foreach (var row in baseElement.Elements("row"))
			{
				i++;
				int j = 0;
				int maxHeight = 0;
				var contentElements = row.Elements();
				var heightAttribute = row.Attribute("height");
				int heightPercent = -1;
				if (heightAttribute != null) heightPercent = int.Parse(heightAttribute.Value.Substring(0, heightAttribute.Value.Length - 1));
				var menuTabRow = new MenuTabRow { RowNumber = i, Elements = new List<MenuTabRow.Element>(), HeightPercent = heightPercent };
				foreach (var element in contentElements)
				{
					j++;
					string id, text, url;
					Bitmap bmp;
					DisplayedReportImage displayedImage;
					string styleAttr;
					bool isBold, smallCaps;
					switch (element.Name?.ToString())
					{
						case "img":
							id = element.Attribute("id")?.Value;
							if (id == null) throw new Exception("img has no id value in xml");
							displayedImage = reports.ReportImages.First(x => x.Id == id);

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

							menuTabRow.Elements.Add(new MenuTabRow.Element { Image = bmp, colNumber = j, Type = MenuTabRow.ElementType.Image });


							break;
						case "table":
							var trows = element.Elements("trow");
							int trowsCount = trows.Count();
							var table = new MenuTabRow.Table { Content = new MenuTabRow.TableCell[trowsCount, trows.First().Elements("tcell").Count()] };
							int k = 0;
							foreach (var trow in trows)
							{
								int l = 0;
								foreach (var tcell in trow.Elements("tcell"))
								{
									MenuTabRow.TableCell cell = new MenuTabRow.TableCell();
									var img = tcell.Elements("img").FirstOrDefault();
									if (img != null)
									{
										id = img.Attribute("id")?.Value;
										if (id == null) throw new Exception("img has no id value in xml");
										displayedImage = reports.ReportImages.First(x => x.Id == id);
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
										cell.Image = bmp;
									}

									styleAttr = tcell.Attribute("fontstyle")?.Value;
									isBold = false;
									smallCaps = false;
									if (!string.IsNullOrEmpty(styleAttr))
									{
										isBold = styleAttr.Contains("bold");
										smallCaps = styleAttr.Contains("small-caps");
									}

									Color bgColor = Color.White;
									var bgColorAttr = tcell.Attribute("bgcolor")?.Value;
									if (bgColorAttr != null)
									{
										if (colorDictionary.TryGetValue(bgColorAttr, out Color color))
										{
											bgColor = color;
										}
										else
										{
											bgColor = ColorTranslator.FromHtml(bgColorAttr);
										}
									}
									var textColor = Color.Black;
									var textColorAttr = tcell.Attribute("textcolor")?.Value;
									if (textColorAttr != null)
									{
										if (colorDictionary.TryGetValue(textColorAttr, out Color color))
										{
											textColor = color;
										}
										else
										{
											textColor = ColorTranslator.FromHtml(textColorAttr);
										}
									}
									cell.BackgroundColor = bgColor;
									cell.TextColor = textColor;
									cell.Content = tcell.Value;
									cell.Tooltip = tcell.Attribute("tooltip")?.Value ?? "";
									cell.IsBold = isBold;
									cell.IsSmallCaps = smallCaps;
									var borderAttr = tcell.Attribute("border")?.Value;
									if (borderAttr != null)
									{
										borderAttr = borderAttr.Trim();
										cell.TopBorder = borderAttr[0] == '1';
										cell.RightBorder = borderAttr[2] == '1';
										cell.BottomBorder = borderAttr[4] == '1';
										cell.LeftBorder = borderAttr[6] == '1';
									}

									var contentAlignment = tcell.Attribute("contentalign")?.Value;
									if (contentAlignment != null)
									{
										var alignment = DataGridViewContentAlignment.MiddleLeft;
										Enum.TryParse(contentAlignment, true, out alignment);
										cell.TextAlign = alignment;
									}

									table.Content[k, l] = cell;
									l++;
								}
								k++;
							}
							menuTabRow.Elements.Add(new MenuTabRow.Element { Table = table, colNumber = j, Type = MenuTabRow.ElementType.Table });
							break;
						case "a":
							url = element.Attribute("href")?.Value;
							text = element.Value;
							var tabRowElement = new MenuTabRow.Element { colNumber = j, Url = url, Text = text, Type = MenuTabRow.ElementType.Link };
							var icon = element.Attribute("icon")?.Value;
							if (icon != null)
							{
								displayedImage = reports.ReportImages.First(x => x.Id == icon);

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
								tabRowElement.Image = bmp;
							}
							if (url != null && text != null)
								menuTabRow.Elements.Add(tabRowElement);

							break;
						case "stattext":
							menuTabRow.Elements.Add(new MenuTabRow.Element { colNumber = j, Type = MenuTabRow.ElementType.StatText });

							break;
						case "label":
							text = element.Value;
							styleAttr = element.Attribute("fontstyle")?.Value;
							isBold = false;
							smallCaps = false;
							if (!string.IsNullOrEmpty(styleAttr))
							{
								isBold = styleAttr.Contains("bold");
								smallCaps = styleAttr.Contains("small-caps");
							}
							menuTabRow.Elements.Add(new MenuTabRow.Element { colNumber = j, Type = MenuTabRow.ElementType.Label, Text = text, IsBold = isBold, IsSmallCaps = smallCaps });
							break;
						case "favoritereport":
							text = element.Value;
							url = element.Attribute("href")?.Value;
							id = element.Attribute("imgid")?.Value;
							if (id == null) throw new Exception("img has no imgid value in xml");
							displayedImage = reports.ReportImages.First(x => x.Id == id);

							using (var ms = new MemoryStream(displayedImage.ReportImage))
							{
								bmp = new Bitmap(ms);
							}

							var tabElement = new MenuTabRow.Element
							{
								colNumber = j,
								Type = MenuTabRow.ElementType.FavoriteReport,
								Image = bmp,
								Text = text,
								Url = url
							};
							menuTabRow.Elements.Add(tabElement);
							break;
						case "featurelink":
							text = element.Value;
							MenuTabRow.Feature feature;
							switch (element.Attribute("feature")?.Value)
							{
								case "messaging":
									feature = MenuTabRow.Feature.Messaging;
									break;
								default:
									feature = default(MenuTabRow.Feature);
									log.Warn($"Unknown feature: {element.Attribute("feature")?.Value}");
									break;
							}
							menuTabRow.Elements.Add(new MenuTabRow.Element { colNumber = j, Type = MenuTabRow.ElementType.FeatureLink, Text = text, FeatureLink = feature });
							break;
						default:
							log.Warn($"Unknown element in xml: {element.Name}");
							break;
					}
				}
				result.Add(menuTabRow);
			}
			return result;
		}
	}
}
