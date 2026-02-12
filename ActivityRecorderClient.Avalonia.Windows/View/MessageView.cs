using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.MessageNotifier;

namespace Tct.ActivityRecorderClient.View
{
	public partial class MessageView : FixedMetroForm
	{
		private readonly Dictionary<int, DisplayedMessage> messageList;
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private volatile bool rendering = false;
		private IMessageService messageService;
		private readonly Regex tableRegex = new Regex(@"<table>\s*(<[\S\s]+>)\s*<\/table>", RegexOptions.IgnoreCase);
		private readonly Regex headerRegex = new Regex(@"<th>\s*(<[\S\s]*>)\s*<\/th>", RegexOptions.IgnoreCase);
		private readonly Regex headerCellRegex = new Regex(@"<td>\s*([^<]*)\s*<\/td>", RegexOptions.IgnoreCase);
		private readonly Regex rowRegex = new Regex(@"<tr>\s*((?:\s*<td>[^<]*<\/td>\s*)+)\s*<\/tr>", RegexOptions.IgnoreCase);
		private readonly Regex cellRegex = new Regex(@"<td>\s*([^<]*)\s*<\/td>", RegexOptions.IgnoreCase);


		public MessageView(IMessageService messageService)
		{
			this.messageService = messageService;
			messageList = new Dictionary<int, DisplayedMessage>();
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
		}

		public void AddMessage(DisplayedMessage message)
		{
			if (messageList.ContainsKey(message.Id))
			{
				messageList[message.Id] = message;
			}
			else
			{
				messageList.Add(message.Id, message);
			}
			refreshMessageListDataGridView();
		}

		public void AddMessages(List<DisplayedMessage> messages)
		{
			foreach (var message in messages)
			{
				if (messageList.ContainsKey(message.Id))
				{
					messageList[message.Id] = message;
				}
				else
				{
					messageList.Add(message.Id, message);
				}
			}
			refreshMessageListDataGridView();
		}

		public void RemoveMessage(int id)
		{
			messageList.Remove(id);
			refreshMessageListDataGridView();
		}

		public IEnumerable<DisplayedMessage> GetMessages()
		{
			return messageList.Values;
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			Hide();
		}

		public void ShowWithId(int? messageId = null)
		{
			messageListDataGridView.Select();
			Show();
			TopMost = true;
			TopMost = false;
			refreshMessageListDataGridView(messageId);
		}

		private void refreshMessageListDataGridView(int? selectedMessageId = null)
		{
			if (!Visible) return;
			if (messageListDataGridView.InvokeRequired)
			{
				messageListDataGridView.Invoke((MethodInvoker)(() => refreshMessageListDataGridView(selectedMessageId)));
				return;
			}
			if (selectedMessageId.HasValue)
			{
				if (!messageList[selectedMessageId.Value].IsRead)
				{
					messageList[selectedMessageId.Value].ReadDate = DateTime.Now;
					ThreadPool.QueueUserWorkItem(_ => messageService.SetPCReadAt(selectedMessageId.Value));
				}
			}
			rendering = true;
			messageListDataGridView.SuspendLayout();
			int? firstDisplayedId = null;
			if (messageListDataGridView.SelectedRows.Count == 1 && selectedMessageId == null)
			{
				if (messageListDataGridView.FirstDisplayedScrollingRowIndex > 0)
					firstDisplayedId = (int)messageListDataGridView.Rows[messageListDataGridView.FirstDisplayedScrollingRowIndex]
						.Cells[0].Value;
				selectedMessageId = (int)messageListDataGridView.SelectedRows[0].Cells[0].Value;
			}

			int? firstDisplayedRowIndex = null;
			int? currentCell = null;
			int idx = 0;
			messageListDataGridView.Rows.Clear();
			var dataGridViewRowList = new List<DataGridViewRow>();
			foreach (var message in messageList.Values.OrderByDescending(c => c.CreatedAt))
			{
				DataGridViewRow dgvr = (DataGridViewRow)messageListDataGridView.RowTemplate.Clone();
				dgvr.CreateCells(messageListDataGridView, "colId", "colDate", "colReasonText");
				dgvr.Cells[0].Value = message.Id;
				dgvr.Cells[1].Value = message.CreatedAt;
				dgvr.Cells[2].Value = message.Content;
				//messageListDataGridView.Rows.Add(message.Id, message.CreatedAt, message.Content);
				DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
				cellStyle.ForeColor = message.IsRead ? Color.SlateGray : Color.Black;
				cellStyle.SelectionForeColor = message.IsRead ? Color.SlateGray : Color.Black;
				dgvr.DefaultCellStyle = cellStyle;
				//messageListDataGridView.Rows[messageListDataGridView.RowCount - 1].DefaultCellStyle = cellStyle;
				dataGridViewRowList.Add(dgvr);

				if (message.Id == selectedMessageId)
				{
					currentCell = idx;
					//messageListDataGridView.Rows[messageListDataGridView.Rows.Count - 1].Selected = true;
				}

				if (firstDisplayedId.HasValue && message.Id == firstDisplayedId.Value)
				{
					firstDisplayedRowIndex = idx;
				}
				idx++;
			}

			messageListDataGridView.Rows.AddRange(dataGridViewRowList.ToArray());
			if (currentCell.HasValue)
			{
				messageListDataGridView.CurrentCell =
					messageListDataGridView.Rows[currentCell.Value].Cells[1];
			}

			displayMessageContent((string)messageListDataGridView.SelectedRows[0].Cells[2].Value);
			if (firstDisplayedRowIndex.HasValue)
			{
				messageListDataGridView.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex.Value;
			}

			rendering = false;
			messageListDataGridView.ResumeLayout();
		}

		private int? lastSelectedId;

		private void messageList_SelectionChanged(object sender, EventArgs e)
		{
			if (rendering) return;
			if (messageListDataGridView.SelectedRows.Count < 1) return;
			int id = (int)messageListDataGridView.SelectedRows[0].Cells[0].Value;
			DisplayedMessage m;
			if (!messageList.TryGetValue(id, out m))
			{
				refreshMessageListDataGridView();
				id = (int)messageListDataGridView.SelectedRows[0].Cells[0].Value;
				m = messageList[id];
			}
			displayMessageContent(m.Content);
			if (lastSelectedId != null)
			{
				var readDate = messageList[(int)lastSelectedId].ReadDate;
				setMessageAsRead(lastSelectedId.Value);
				if (readDate == null)
					refreshMessageListDataGridView();
			}

			lastSelectedId = id;
		}

		private void displayMessageContent(string content)
		{
			List<string> tables = convertHtmlTablesToRtfTables(ref content);
			string result = "{\\rtf1 " + GetRtfUnicodeEscapedString(content) + "}";
			result = Regex.Replace(result, "<b>", "\\b ");
			result = Regex.Replace(result, "<\\/b>", "\\b0 ");
			result = Regex.Replace(result, "<i>", "\\i ");
			result = Regex.Replace(result, "<\\/i>", "\\i0 ");
			result = Regex.Replace(result, "\n", "\\line ");
			for (int i = 0; i < tables.Count; i++)
			{
				result = result.Replace("#table" + i + "#", tables[i]);
			}

			if (messageViewerRichTextBox.InvokeRequired)
			{
				messageViewerRichTextBox.Invoke((MethodInvoker)(() =>setRichTextBoxText(result)));
			}
			else
			{
				setRichTextBoxText(result);
			}
		}

		private List<string> convertHtmlTablesToRtfTables(ref string s)
		{
			List<string> result = new List<string>();
			Match tableMatch = tableRegex.Match(s);
			int counter = 0;
			while (tableMatch.Success)
			{
				int columnCount = 0;
				int rowCount = 1;
				List<string> headerCells = new List<string>();
				List<List<string>> rowCells = new List<List<string>>();

				var tablecontent = tableMatch.Groups[1].Value;
				Match headerMatch = headerRegex.Match(tablecontent);

				if (headerMatch.Success)
				{
					var headerContent = headerMatch.Groups[1].Value;
					Match headerCellMatch = headerCellRegex.Match(headerContent);
					while (headerCellMatch.Success)
					{
						columnCount++;
						headerCells.Add(headerCellMatch.Groups[1].Value);
						headerCellMatch = headerCellMatch.NextMatch();
					}
				}
				else
				{
					Match firstRowMatch = rowRegex.Match(tablecontent);
					var firstRowContent = firstRowMatch.Groups[1].Value;
					Match firstRowCellMatch = cellRegex.Match(firstRowContent);
					rowCells.Add(new List<string>());
					while (firstRowCellMatch.Success)
					{
						columnCount++;
						rowCells[0].Add(firstRowCellMatch.Groups[1].Value);
						firstRowCellMatch = firstRowCellMatch.NextMatch();
					}
					tablecontent = tablecontent.Substring(firstRowMatch.Index + firstRowMatch.Length);
				}

				Match rowMatch = rowRegex.Match(tablecontent);
				while (rowMatch.Success)
				{
					var rowContent = rowMatch.Groups[1].Value;
					Match cellMatch = cellRegex.Match(rowContent);
					rowCells.Add(new List<string>(columnCount));
					while (cellMatch.Success)
					{
						rowCells[rowCells.Count - 1].Add(cellMatch.Groups[1].Value);
						cellMatch = cellMatch.NextMatch();
					}
					rowMatch = rowMatch.NextMatch();
				}

				int[] columnSizes = new int[columnCount];
				for (int i = 0; i < rowCells.Count; i++)
				{
					for (int j = 0; j < rowCells[i].Count; j++)
					{
						string currCellContent = rowCells[i][j];
						var size = TextRenderer.MeasureText(currCellContent, messageViewerRichTextBox.Font);
						if (size.Width > columnSizes[j]) columnSizes[j] = size.Width;
					}
				}
				StringBuilder res = new StringBuilder();
				int wholeColSizes = 0;
				if (headerCells.Count > 0)
				{

					Font f = messageViewerRichTextBox.Font;
					Font newFont = new Font(f.FontFamily, f.Size, FontStyle.Bold, f.Unit, f.GdiCharSet);

					for (int i = 0; i < headerCells.Count; i++)
					{
						string currCellContent = headerCells[i];
						var size = TextRenderer.MeasureText(currCellContent, newFont);
						if (size.Width > columnSizes[i]) columnSizes[i] = size.Width;
					}


					res.Append("\\par\\pard\\par\\pard\\b\\trowd\\trgaph72\r\n");
					StringBuilder headerCellBuilder = new StringBuilder();
					for (int i = 0; i < columnCount; i++)
					{
						res.Append("\\cellx");
						wholeColSizes += columnSizes[i] * 15 + 100;
						res.Append(wholeColSizes);
						res.Append("\r\n");
						headerCellBuilder.Append("\\intbl ");
						headerCellBuilder.Append(headerCells[i]);
						headerCellBuilder.Append("\\cell\r\n");
					}

					res.Append(headerCellBuilder);
					res.Append("\\row\\b0\r\n");
				}

				foreach (var cellsInRow in rowCells)
				{
					StringBuilder cellBuilder = new StringBuilder();
					res.Append("\\trowd\\trgaph72\r\n");
					wholeColSizes = 0;
					for (int i = 0; i < columnCount; i++)
					{
						res.Append("\\cellx");
						wholeColSizes += columnSizes[i] * 15 + 100;
						res.Append(wholeColSizes);
						res.Append("\r\n");
						cellBuilder.Append("\\intbl ");
						cellBuilder.Append(cellsInRow[i]);
						cellBuilder.Append("\\cell\r\n");
					}

					res.Append(cellBuilder);
					res.Append("\\row\r\n");
				}

				res.Append("\\pard\\par");

				var builder = new StringBuilder();
				builder.Append(s.Substring(0, tableMatch.Index));
				builder.Append("#table" + counter++ + "#");
				builder.Append(s.Substring(tableMatch.Index + tableMatch.Length));
				s = builder.ToString();
				result.Add(res.ToString());
				tableMatch = tableMatch.NextMatch();
			}

			return result;
		}

		private void setRichTextBoxText(string rtfText)
		{
			foreach (LinkLabel linkLabel in messageViewerRichTextBox.Controls.OfType<LinkLabel>())
			{
				messageViewerRichTextBox.Controls.Remove(linkLabel);
			}
			messageViewerRichTextBox.Rtf = rtfText;
			var matches = Regex.Matches(messageViewerRichTextBox.Text, "<a\\s+(?:[^>]*?\\s+)?href=([\"'])(.*?)\\1[^>]*>([^<]*)<\\/a>");
			foreach (Match match in matches)
			{
				var m = match.Value;
				var index = messageViewerRichTextBox.Text.IndexOf(m, StringComparison.Ordinal);

				LinkLabel link = new LinkLabel { Text = match.Groups[3].Value };
				link.LinkClicked += link_LinkClicked;
				LinkLabel.Link data = new LinkLabel.Link { LinkData = match.Groups[2].Value };
				link.Links.Add(data);
				link.AutoSize = true;
				link.Location = messageViewerRichTextBox.GetPositionFromCharIndex(index);
				messageViewerRichTextBox.Text = messageViewerRichTextBox.Text.Remove(index, m.Length);
				messageViewerRichTextBox.Text = messageViewerRichTextBox.Text.Insert(index, link.Text + "   ");
				messageViewerRichTextBox.Controls.Add(link);
				messageViewerRichTextBox.SelectionStart = messageViewerRichTextBox.TextLength;

			}
		}

		static string GetRtfUnicodeEscapedString(string s)
		{
			var sb = new StringBuilder();
			foreach (var c in s)
			{
				if (c == '\\' || c == '{' || c == '}')
					sb.Append(@"\" + c);
				else if (c <= 0x7f)
					sb.Append(c);
				else
					sb.Append("\\u" + Convert.ToUInt32(c) + "?");
			}
			return sb.ToString();
		}


		private void setMessageAsRead(int id)
		{
			if (messageList[id].ReadDate == null)
				messageList[id].ReadDate = messageService.SetPCReadAt(id);
		}

		private void MessageView_Load(object sender, EventArgs e)
		{
			Text = Labels.Messages;
			closeButton.Text = Labels.Close;
			messageListDataGridView.Columns["colDate"].HeaderText = Labels.CreatedAt;
			messageListDataGridView.Columns["colReasonText"].HeaderText = Labels.Content;
			refreshMessageListDataGridView();
		}

		private void MessageView_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void messageListDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			setMessageAsRead((int)messageListDataGridView[0, e.RowIndex].Value);
			refreshMessageListDataGridView();
		}

		private void messageViewerRichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					var url = e.LinkText;
					if (!url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
					{
						var ticket = AuthenticationHelper.GetAuthTicket();
						url = string.Format(ConfigManager.WebsiteUrl + "Account/Login.aspx?ticket={0}&url=", ticket) + Uri.EscapeDataString(url);
					}
					System.Diagnostics.Process.Start(url);
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + e.LinkText, ex);
				}
			});
		}

		private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var url = "";
				try
				{
					url = e.Link.LinkData as string;
					if (!url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
					{
						var ticket = AuthenticationHelper.GetAuthTicket();
						url = string.Format(ConfigManager.WebsiteUrl + "Account/Login.aspx?ticket={0}&url=", ticket) + Uri.EscapeDataString(url);
					}
					System.Diagnostics.Process.Start(url);
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}
	}
}
