using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public partial class ContributionForm : MetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IList<Snippet> data;
		public static event EventHandler<SingleValueEventArgs<string>> OnRemoveItem;
		public static event EventHandler FormClosing;
		public IEnumerable<Snippet> Resolutions
		{
			get
			{
				int CONTENT_COLUMN = 2;
				for (var row = 1; row <= tblSamples.RowCount; row++)
				{
					var control = tblSamples.GetControlFromPosition(CONTENT_COLUMN, row);
					if (!(control is TextBox)) continue;
					var item = data.SingleOrDefault(e => e.Guid == (Guid)((TextBox)control).Tag);
					if (item != null && !string.IsNullOrEmpty(control.Text))
					{
						item.Content = control.Text;
					}
					yield return item;
				}
			}
		}
		public ContributionForm(IList<Snippet> data)
		{
			InitializeComponent();
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			btnCancel.Text = Labels.Cancel;
			btnOk.Text = Labels.Ok;
			btnClear.Text = Labels.Delete;
			tblSamples.ColumnStyles.Clear();
			this.data = data;
		}
		private void ContributionForm_Load(object sender, EventArgs e)
		{
			tblSamples.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			tblSamples.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
			tblSamples.ColumnCount = 3;
			tblSamples.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			tblSamples.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
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
			rc++;

			foreach (var item in data)
			{
				tblSamples.RowStyles.Add(new RowStyle());
				var pb = new PictureBox { SizeMode = PictureBoxSizeMode.AutoSize, Name = "img" + rc, Tag = item.Guid };
				var pbTrash = new PictureBox
				{
					SizeMode = PictureBoxSizeMode.AutoSize,
					Image = Resources.btn_delete,
					Name = "trash" + rc,
					Tag = item.Guid,
					Cursor = Cursors.Hand
				};
				pbTrash.Click += (s, __) =>
				{
					var trash = s as PictureBox;
					if (trash == null) return;
					Guid id = Guid.Parse(trash.Tag.ToString());
					var coll = tblSamples.Controls.OfType<Control>().Where(c => Guid.Parse(c.Tag.ToString()) == id);
					if (!coll.Any()) return;
					for (int i = coll.Count() - 1; 0 <= i; i--)
					{
						var ctrl = coll.ElementAt(i);
						tblSamples.Controls.Remove(ctrl);
					}
					var itemToRemove = data.FirstOrDefault(d => d.Guid == id);
					if (itemToRemove == null) return;
					data.Remove(itemToRemove);
					var handler = OnRemoveItem;
					if (handler != null)
						handler(this, new SingleValueEventArgs<string>(itemToRemove.ImageFileName));
				};
				var tb = new TextBox
				{
					Text = item.Content,
					Font = new Font("Verdana", 11F, FontStyle.Regular, GraphicsUnit.Point, 238),
					AcceptsReturn = true,
					Dock = DockStyle.Fill,
					Name = "text" + rc,
					Tag = item.Guid
				};
				tblSamples.Controls.Add(pbTrash, 0, rc);
				tblSamples.Controls.Add(pb, 1, rc);
				tblSamples.Controls.Add(tb, 2, rc);
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
				tb.KeyDown += (s, ea) =>
				{
					if (ea.KeyCode == Keys.Enter)
						SendKeys.Send("{TAB}");
				};
				tb.Enter += (s, ea) =>
				{
					((TextBox)s).SelectAll();
				};
				if (rc == 0) tb.Focus();		//TODO it does not work, cursor do not jump to first textbox either
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
			if (FormClosing != null)
				FormClosing.Invoke(this, EventArgs.Empty);
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