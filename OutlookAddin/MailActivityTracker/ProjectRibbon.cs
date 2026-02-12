using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Word;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderClient.View;
using UIAutomationClient;

namespace MailActivityTracker
{
	using log4net;
	using Microsoft.Office.Interop.Outlook;
	using Microsoft.Office.Tools.Ribbon;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	public partial class ProjectRibbon
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal static ClientMenu ClientMenu { set; get; }
		public static string PlaceHolder { set; get; }
		private static object lockObj = new object();
		private IUIAutomationElement cachedSearchBox;
		private void ProjectRibbon_Load(object sender, RibbonUIEventArgs e)
		{
			try
			{
				if (ClientMenu == null)
				{
					grpJC.Visible = false;
					return;
				}
				menu1.Label = Labels.TaskSelector;
				label1.Label = Labels.SearchForWork;
				grpJC.Visible = true;
				grpJC.Label = PlaceHolder;
				log.Info("ProjectRibbon has been loaded");
				lock (lockObj)
					foreach (var m in ClientMenu.Works)
						MenuBuilder(menu1, m);
				ThreadPool.QueueUserWorkItem(_ =>
				{
					flattenWorkData = flattenWorkDatas(ClientMenu.Works);
				});
			}
			catch (System.Exception ex)
			{
				log.Error("ProjectRibbon loading failed", ex);
			}
		}
		private void MenuBuilder(RibbonMenu actLevel, Tct.ActivityRecorderService.WorkData actWork)
		{
			if (actWork.Children == null || actWork.Children.Count == 0)
			{
				RibbonButton button;
				actLevel.Items.Add(button = Factory.CreateRibbonButton());
				button.Label = actWork.Name;
				button.Click += b_Click;
				button.SuperTip = actWork.Description;
				button.Tag = actWork;
			}
			else
			{
				RibbonMenu menu;
				if (actWork.IsReadOnly)
					menu = actLevel;
				else
				{
					actLevel.Items.Add(menu = Factory.CreateRibbonMenu());
					menu.Dynamic = true;
					menu.Label = actWork.Name;
					if (actWork.Children != null && actWork.Children.Count == 1)
					{
						var oneLevel = false;
						menu.Label += Concat(actWork.Children, out oneLevel);
						if (oneLevel)
							actWork = actWork.Children[0];
					}
				}
				foreach (var ch in actWork.Children)
					MenuBuilder(menu, ch);
			}
		}
		private string Concat(List<Tct.ActivityRecorderService.WorkData> list, out bool oneLevel)
		{
			if (list.Count == 1 && list[0].Children != null && list[0].Children.Count > 0)
			{
				list[0].IsReadOnly = true;
				oneLevel = true;
				return " » " + list[0].Name;
			}
			oneLevel = false;
			foreach (var w in list)
				if (w.Children != null && w.Children.Count == 1 && w.Children.Count > 0)
				{
					w.IsReadOnly = true;
					return " » " + w.Name + Concat(w.Children, out oneLevel);
				}
			return null;
		}

		void b_Click(object sender, RibbonControlEventArgs e)
		{
			dynamic editorObj = null;
			dynamic context = null;
			try
			{
				var application = Globals.ThisAddIn.Application; // these are global object can't be released
				var inspector = application.ActiveInspector();
				Tct.ActivityRecorderService.WorkData tmp = null;
				if (sender is RibbonButton) tmp = (sender as RibbonButton).Tag as Tct.ActivityRecorderService.WorkData;
				context = e.Control.Context;
				if (sender is System.Windows.Forms.ListBox && context is ListBoxItem)
				{
					var ei = context as ListBoxItem;
					tmp = new Tct.ActivityRecorderService.WorkData { Id = ei.Id, Name = ei.Title };
				}
				if (tmp == null) return;
				try
				{
					editorObj = inspector.WordEditor;
				}
				catch (COMException ex)
				{
					log.Warn("Accessing WordEditor failed", ex);
					editorObj = null;
				}
				var editor = editorObj as Document;
				dynamic currentItem = null;
				try
				{
					currentItem = inspector.CurrentItem;
					if (ThisAddIn.TaskIdSettings.HasFlag(Model.MeetingPluginTaskIdSettings.Subject))
					{
						if (currentItem is MeetingItem) ((MeetingItem)currentItem).Subject = UpdateBody(((MeetingItem)currentItem).Subject, tmp);
						else if (currentItem is AppointmentItem) ((AppointmentItem)currentItem).Subject = UpdateBody(((AppointmentItem)currentItem).Subject, tmp);
						else if (currentItem is MailItem) ((MailItem)currentItem).Subject = UpdateBody(((MailItem)currentItem).Subject, tmp);
					}
					if (editor == null)
					{
						if (ThisAddIn.TaskIdSettings.HasFlag(Model.MeetingPluginTaskIdSettings.Description))
						{
							if (currentItem is MeetingItem) ((MeetingItem)currentItem).Body = UpdateBody(((MeetingItem)currentItem).Body, tmp);
							else if (currentItem is AppointmentItem) ((AppointmentItem)currentItem).Body = UpdateBody(((AppointmentItem)currentItem).Body, tmp);
							else if (currentItem is MailItem) ((MailItem)currentItem).Body = UpdateBody(((MailItem)currentItem).Body, tmp);
						}
						return;
					}
				}
				catch (System.Exception ex)
				{
					log.Debug("Unexpected exception when writing the id", ex);
					return;
				}
				finally
				{
					if (currentItem != null)
						Marshal.ReleaseComObject(currentItem);
				}
				if (!ThisAddIn.TaskIdSettings.HasFlag(Model.MeetingPluginTaskIdSettings.Description)) return;
				Range range = null;
				Find find = null;
				Range content = null;
				Range rangeRplc = null;
				Find findRplc = null;
				try
				{
					range = editor.Range();
					find = range.Find;
					if (!find.Execute($@"\[{PlaceHolder}#[0-9]@\]", MatchWildcards: true))
					{
						content = editor.Content;
						content.InsertAfter(FormatClipboardData(tmp));
						return;
					}
					var text = range.Text;
					var cutRegex = new Regex("#(\\d+)\\]");
					var match = cutRegex.Match(text);
					if (!int.TryParse(match.Groups[1].Value, out var id)) return;
					flattenWorkData = flattenWorkDatas(ClientMenu.Works);
					var oldWorkData = flattenWorkData.FirstOrDefault(d => d.Id == id);
					if (oldWorkData == null) return;
					rangeRplc = editor.Range();
					findRplc = rangeRplc.Find;
					findRplc.Execute(FormatClipboardData(oldWorkData).Trim(), ReplaceWith: FormatClipboardData(tmp).Trim(), Replace: WdReplace.wdReplaceAll);
				}
				finally
				{
					if (findRplc != null) Marshal.ReleaseComObject(findRplc);
					if (rangeRplc != null) Marshal.ReleaseComObject(rangeRplc);
					if (content != null) Marshal.ReleaseComObject(content);
					if (find != null) Marshal.ReleaseComObject(find);
					if (range != null) Marshal.ReleaseComObject(range);
				}
			}
			catch (System.Exception ex)
			{
				log.Error("insert workdata failed", ex);
			}
			finally
			{
				if (context != null) Marshal.ReleaseComObject(context);
				if (editorObj != null) Marshal.ReleaseComObject(editorObj);
			}
		}

		private static List<WorkData> flattenWorkData;
		private static string UpdateBody(string body, WorkData newWorkData)
		{
			if (body == null) return FormatClipboardData(newWorkData);
			body = body.Replace("–", "-");
			var rex = new Regex(string.Format(@"(.*)\[{0}#(\d*)\](.*)", PlaceHolder));
			var match = rex.Match(body);
			if (!match.Success) return body + " " + FormatClipboardData(newWorkData);
			int id;
			if (!int.TryParse(match.Groups[2].Value, out id)) return body;
			flattenWorkData = flattenWorkDatas(ClientMenu.Works);
			var oldWorkData = flattenWorkData.FirstOrDefault(e => e.Id == id);
			return body.Replace(
				FormatClipboardData(oldWorkData),
				FormatClipboardData(newWorkData));
		}
		private static List<WorkData> flattenWorkDatas(List<WorkData> works)
		{
			var res = new List<WorkData>();
			var que = new Queue<List<WorkData>>();
			que.Enqueue(works);
			while (que.Count > 0)
			{
				var item = que.Dequeue();
				res.AddRange(item);
				foreach (var wdWc in item.Where(i => i.Children != null))
				{
					que.Enqueue(wdWc.Children);
				}
			}
			return res;
		}
		private static string FormatClipboardData(Tct.ActivityRecorderService.WorkData wd)
		{
			return FormatClipboardData(wd.Id, wd.Name);
		}
		private static string FormatClipboardData(int? workId, string workName)
		{
			return string.Format(" [{0}#{1}] {2}", PlaceHolder, workId.HasValue ? workId.Value.ToString() : "", workName);
		}
		private void editBox1_TextChanged(object sender, RibbonControlEventArgs ex)
		{
			var eb = sender as RibbonEditBox;
			if (string.IsNullOrEmpty(eb.Text)) return;
			if (flattenWorkData == null || flattenWorkData.Count == 0) return;
			var s = eb.Text.ToLower(CultureInfo.InvariantCulture);
			var result = from e in flattenWorkData
						 where e.Id.HasValue && e.Name.ToLower(CultureInfo.InvariantCulture).IndexOf(s) > -1
						 select new ListBoxItem
						 {
							 Id = e.Id ?? -1,
							 Title = e.Name,
							 Description = e.Description
						 };
			if (cachedSearchBox == null)
			{
				CUIAutomation auto = null;
				IUIAutomationElement elementAppWindow = null;
				IUIAutomationElementArray subtree = null;
				try
				{
					var hWnd = WinApi.GetForegroundWindow();
					auto = new CUIAutomation();
					elementAppWindow = auto.ElementFromHandle(hWnd);
					var trueCondition = auto.CreateTrueCondition();
					subtree = elementAppWindow.FindAll(TreeScope.TreeScope_Subtree, trueCondition);
					for (var i = 0; i < subtree.Length; i++)
					{
						var element = subtree.GetElement(i);
						if (element.CurrentClassName == "NetUITextbox" && element.CurrentName == ebTaskSelector.Name)
						{
							if (cachedSearchBox != null) Marshal.ReleaseComObject(cachedSearchBox); // release prev cached element
							cachedSearchBox = element;
							break;
						}
						log.DebugFormat("{0}/{1}/{2}/{3}", element.CurrentControlType, element.CurrentClassName, element.CurrentItemType, element.CurrentName);
						Marshal.ReleaseComObject(element);
					}
				}
				finally
				{
					if (subtree != null) Marshal.ReleaseComObject(subtree);
					if (elementAppWindow != null) Marshal.ReleaseComObject(elementAppWindow);
					if (auto != null) Marshal.ReleaseComObject(auto);
				}
				if (cachedSearchBox == null)
				{
					log.Error("Outlook ribbon does not contains the appropriate searcbox control.");
					return;
				}
			}
			if (!result.Any())
				result = GetNotfound;
			new TaskSelector(result,
				b_Click,
				() => { ebTaskSelector.Text = ""; },
				cachedSearchBox.CurrentBoundingRectangle.left - 1,
				cachedSearchBox.CurrentBoundingRectangle.bottom + 1).Show();
		}
		private IEnumerable<ListBoxItem> GetNotfound
		{
			get
			{
				yield return new ListBoxItem
				{
					Title = Labels.NotFound,
					Id = -1
				};
			}
		}
	}
}

namespace Tct.ActivityRecorderService
{
	internal partial class WorkData
	{
		public bool Processed { get; set; }
	}
}