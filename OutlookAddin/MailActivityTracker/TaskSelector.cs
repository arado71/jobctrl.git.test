using Microsoft.Office.Tools.Ribbon;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.View;

namespace MailActivityTracker
{
    public partial class TaskSelector : Form
    {
        private readonly Action<object, RibbonControlEventArgs> onSelected;
        private readonly Action onClosed;
        private readonly List<ListBoxItem> taskList;
        private readonly int posX, posY;
        public TaskSelector(IEnumerable<ListBoxItem> taskList, Action<object, RibbonControlEventArgs> onSelected, Action onClosed, int posX, int posY)
        {
            InitializeComponent();
            lbTask.DataSource = this.taskList = taskList.ToList();
            this.onSelected = onSelected;
            this.posX = posX;
            this.posY = posY;
            this.onClosed = onClosed;
            lbTask.DrawMode = DrawMode.OwnerDrawVariable;
            lbTask.MeasureItem += lbTask_MeasureItem;
            lbTask.DrawItem += lbTask_DrawItem;
        }
        private void TaskSelector_Load(object sender, EventArgs e)
        {
            Top = posY;
            Left = posX;
        }
        private void TaskSelector_FormClosed(object sender, FormClosedEventArgs e)
        {
            onClosed();
        }
        private void TaskSelector_Deactivate(object sender, EventArgs e)
        {
            Close();
        }
        void lbTask_DrawItem(object sender, DrawItemEventArgs e)
        {
			e.DrawBackground();
            ListBoxItem item = lbTask.Items[e.Index] as ListBoxItem;
			using (var redBrush = new SolidBrush(Color.Red))
			using (var foreColorBrush = new SolidBrush(e.ForeColor))
			using (var bold = new Font(e.Font, FontStyle.Bold))
			{
				if (item == null)
					e.Graphics.DrawString("-empty-", e.Font, redBrush, e.Bounds);
				else
				{
					bool hasDescription = !string.IsNullOrEmpty(item.Description);
					SizeF s = e.Graphics.MeasureString(item.Title, bold);
					RectangleF rTitle = new RectangleF(e.Bounds.Left + 1, e.Bounds.Top + 1, e.Bounds.Right - 1, e.Bounds.Bottom - (hasDescription ? s.Height : 0));
					e.Graphics.DrawString(item.Title, bold, item.Id == -1 ? redBrush : foreColorBrush, rTitle);
					if (hasDescription)
					{
						RectangleF rDescr = new RectangleF(e.Bounds.Left + 10, e.Bounds.Top + s.Height + 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);
						e.Graphics.DrawString(item.Description, e.Font, foreColorBrush, rDescr);
					}
				}
			}
            e.DrawFocusRectangle();
        }
        private void lbTask_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (!(sender is ListBox)) return;
            ListBoxItem item = lbTask.Items[e.Index] as ListBoxItem;
            e.ItemHeight = string.IsNullOrEmpty(item.Description) ? 20 : 40;
        }
        private void lbTask_SelectedIndexChanged(object sender)
        {
            var item = taskList[lbTask.SelectedIndex];
            if (item.Id > -1)                                        // "notfound item" has -1 for Id
                onSelected(sender, new RibbonEventArgImpl(item));
            Close();
        }
        private void lbTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                lbTask_SelectedIndexChanged(sender);
        }
        private void lbTask_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                return;
            if (e.KeyCode == Keys.Escape)
                Close();
        }
        private void lbTask_Click(object sender, EventArgs e)
        {
            lbTask_SelectedIndexChanged(sender);
        }
    }
    public class ListBoxItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return string.Format("Id:{0} Title:{1} Desc:{2}", Id, Title, Description);
        }
    }
    internal class RibbonEventArgImpl : RibbonControlEventArgs
    {
        private readonly ListBoxItem item;
        public RibbonEventArgImpl(ListBoxItem selected)
        {
            item = selected;
        }
        public Microsoft.Office.Core.IRibbonControl Control
        {
            get { return new RibbonControlImpl(item); }
        }
    }
    internal class RibbonControlImpl : Microsoft.Office.Core.IRibbonControl
    {
        private readonly ListBoxItem selected;

        public RibbonControlImpl(ListBoxItem selected)
        {
            this.selected = selected;
        }
        public dynamic Context
        {
            get { return selected; }
        }

        public string Id
        {
            get { return Guid.NewGuid().ToString(); }
        }

        public string Tag
        {
            get { throw new NotImplementedException(); }
        }
    }
}
