using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.View.ToolStrip;
using log4net;
using Tct.ActivityRecorderClient.Menu;
using System.Web;
using Tct.ActivityRecorderClient.Configuration;

namespace Tct.ActivityRecorderClient.View
{
	public static class ClipboardHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void SetClipboardData(int workId, string workName)
		{
			try
			{
				var data = new DataObject();
				data.SetData(DataFormats.UnicodeText, FormatClipboardData(workId, workName));
				data.SetData(DataFormats.Html, FormatClipboardDataHtml(workId, workName));
				Clipboard.SetDataObject(data, true);
			}
			catch (Exception ex)
			{
				log.Error("Unable to set clipboard data", ex);
			}
		}

		public static void SetClipboardData(WorkDataWithParentNames data)
		{
			if (data == null || data.WorkData == null || !data.WorkData.Id.HasValue) return;
			SetClipboardData(data.WorkData.Id.Value, data.FullName);
		}

		private static string FormatClipboardData(int workId, string workName)
		{
			return string.Format(" [{0}#{1}] {2}", AppConfig.Current.TaskPlaceholder, workId, workName);
		}

		private static Stream FormatClipboardDataHtml(int workId, string workName)
		{
			return HtmlClipboardHelper.GetClipboardData(
				"[<A href=\"" + ConfigManager.WebsiteUrl + "UserCenter/Tasks/Default.aspx?TaskId=" + workId + "\">JobCTRL #" + workId + "</A>] " +
				HttpUtility.HtmlEncode(workName));
		}

		public static void HandleCtrlCKeyDownForDropDown(object sender, KeyEventArgs e, ClientMenuLookup clientMenuLookup)
		{
			if (e.KeyData != (Keys.Control | Keys.C)) return;

			var dropDown = sender as ToolStripDropDown;
			if (dropDown != null)
			{
				SetClipBoardDataForToolStripItemCollection(dropDown.Items, clientMenuLookup);
			}
		}

		private static bool SetClipBoardDataForToolStripItemCollection(ToolStripItemCollection items, ClientMenuLookup clientMenuLookup)
		{
			foreach (var item in items.OfType<ToolStripMenuItemWithButton>())
			{
				Debug.Assert(item.WorkData != null);
				if (!item.Selected) continue;
				if (item.WorkData == null || !item.WorkData.Id.HasValue) break;
				var work = clientMenuLookup.GetWorkDataWithParentNames(item.WorkData.Id.Value);
				ClipboardHelper.SetClipboardData(work);
				return true;
			}
			return false;
		}

		public class HtmlClipboardHelper
		{
			private const string html1 = @"Version:1.0
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
";
			private const string html2 = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<HTML>
<head>
<title>HTML clipboard</title>
</head>
<body>
<!--StartFragment-->";
			private const string html3 = @"<!--EndFragment-->
</body>
</html>";
			private static readonly int html1Count = Encoding.UTF8.GetByteCount(html1);
			private static readonly int html2Count = Encoding.UTF8.GetByteCount(html2);
			private static readonly int html3Count = Encoding.UTF8.GetByteCount(html3);

			public static Stream GetClipboardData(string bodyHtml)
			{
				var contentSize = Encoding.UTF8.GetByteCount(bodyHtml);
				var sb = new StringBuilder().Append(html1);
				sb.Replace("<<<<<<<1", html1Count.ToString("D8"));
				sb.Replace("<<<<<<<2", (html1Count + html2Count + contentSize + html3Count).ToString("D8"));
				sb.Replace("<<<<<<<3", (html1Count + html2Count).ToString("D8"));
				sb.Replace("<<<<<<<4", (html1Count + html2Count + contentSize).ToString("D8"));
				sb.Append(html2);
				sb.Append(bodyHtml);
				sb.Append(html3);
				return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
			}
		}
	}
}
