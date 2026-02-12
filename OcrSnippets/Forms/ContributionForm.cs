using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MetroFramework.Forms;
using TcT.ActivityRecorderClient.Capturing.Plugins.Ocr;

namespace TcT.OcrSnippets
{
    public partial class ContributionForm : MetroForm
    {
        private readonly IList<ContributionItem> data;

        public ContributionForm(IList<ContributionItem> data)
        {
            InitializeComponent();
            tblSamples.ColumnStyles.Clear();
            this.data = data;
        }

        public IEnumerable<ContributionItem> Resolutions
        {
            get
            {
                for (var row = 1; row <= tblSamples.RowCount; row++)
                {
					//content
                    var control = tblSamples.GetControlFromPosition(2, row);
					ContributionItem item = null;
                    if (control is TextBox)
					{
						item = data.SingleOrDefault(e => e.Guid == (Guid)control.Tag);
						if (item != null && !string.IsNullOrEmpty(control.Text))
							item.Content = control.Text;
					}
						
					//title
					var controlTitle = tblSamples.GetControlFromPosition(3, row);
					if (controlTitle is TextBox)
					{
						item = data.SingleOrDefault(e => e.Guid == (Guid)controlTitle.Tag);
						if (item != null && !string.IsNullOrEmpty(controlTitle.Text))
							item.WindowTitle = controlTitle.Text;
					}
					if (item == null) continue;
					yield return item;
                }
            }
        }

        public static event EventHandler FormClosing;

        private void ContributionForm_Load(object sender, EventArgs e)
        {
            tblSamples.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tblSamples.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            tblSamples.ColumnCount = 4;
            tblSamples.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblSamples.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblSamples.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tblSamples.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			var rc = 0;
            var formWidthOriginal = Size.Width;
            int pictureBoxWidthOriginal = 50, maxWidth = 0;

			tblSamples.RowStyles.Add(new RowStyle());
			var lbImage = new Label()
			{
				Text = "Image",
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 238),
				Tag = Guid.NewGuid()
			};
			tblSamples.Controls.Add(lbImage, 1, rc);
			var lbContent = new Label()
			{
				Text = "Content",
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 238),
				Tag = Guid.NewGuid()
			};
			tblSamples.Controls.Add(lbContent, 2, rc);
			var lbTitleFilter = new Label()
			{
				Text = "Title Regex",
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 238),
				Tag = Guid.NewGuid()
			};
			tblSamples.Controls.Add(lbTitleFilter, 3, rc);
			rc++;

			foreach (var item in data)
            {
                tblSamples.RowStyles.Add(new RowStyle());
                var pbTrash = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.AutoSize,
                    Image = OcrSnippets.Properties.Resources.btn_delete,
                    Name = "trash" + rc,
                    Tag = item.Guid,
                    Cursor = Cursors.Hand
                };
                pbTrash.Click += (s, __) =>
                {
                    var trash = s as PictureBox;
	                if (trash == null) return;
                    Guid id = Guid.Parse(trash.Tag.ToString());
                    var coll = tblSamples.Controls.OfType<Control>().Where(c => Guid.Parse(c.Tag.ToString()) == id).ToArray();
                    for (int i = coll.Length - 1; 0 <= i; i--)
                    {
                        var ctrl = coll.ElementAt(i);
                        tblSamples.Controls.Remove(ctrl);
                    }
                    var itemToRemove = data.FirstOrDefault(d => d.Guid == id);
                    data.Remove(itemToRemove);
                };
                var pb = new PictureBox { SizeMode = PictureBoxSizeMode.AutoSize, Name = "img" + rc, Tag = item.Guid };
                var tbContent = new TextBox
                {
                    Text = item.Content,
                    Font = new Font("Verdana", 11F, FontStyle.Regular, GraphicsUnit.Point, 238),
                    AcceptsReturn = true,
                    Dock = DockStyle.Fill,
                    Name = "text" + rc,
                    Tag = item.Guid
                };
				var tbTitle = new TextBox
				{
					Text = Regex.Escape(item.WindowTitle),
					Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 238),
					AcceptsReturn = true,
					Dock = DockStyle.Fill,
					Name = "textTitle" + rc,
					Tag = item.Guid,
				};
				tblSamples.Controls.Add(pbTrash, 0, rc);
                tblSamples.Controls.Add(pb, 1, rc);
                tblSamples.Controls.Add(tbContent, 2, rc);
				tblSamples.Controls.Add(tbTitle, 3, rc);
				if (rc == 0)
                {
                    var pos = tblSamples.GetCellPosition(pb);
                    pictureBoxWidthOriginal = tblSamples.GetColumnWidths()[pos.Column];
                }
                pb.SizeChanged += (o, args) =>
                {
                    if (((PictureBox)o).Size.Width > maxWidth)
                        maxWidth = ((PictureBox)o).Size.Width;
                };
                pb.Image = item.Image;
				tbContent.KeyDown += (s, ea) =>
                {
                    if (ea.KeyCode == Keys.Enter)
                        SendKeys.Send("{TAB}");
                };
                if (rc == 0) tbContent.Focus();
                tblSamples.RowCount++;
                if (rc++ > 1)
                    tblSamples.RowStyles.Add(new RowStyle());
            }
            if (maxWidth > pictureBoxWidthOriginal)
                Size = new Size(formWidthOriginal + maxWidth - pictureBoxWidthOriginal, Size.Height);
            CenterToScreen();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            FormClosing?.Invoke(this, EventArgs.Empty);
            base.OnClosing(e);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }
    }
}