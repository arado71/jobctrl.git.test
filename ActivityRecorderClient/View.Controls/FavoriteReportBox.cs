using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class FavoriteReportBox : UserControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Image Image { get; set; }
		public new string ReportName { get; set; }
		public string Url { get; set; }

		public FavoriteReportBox()
		{
			InitializeComponent();
		}

		public FavoriteReportBox(string name, Image image, string url)
		{
			ReportName = name;
			Image = image;
			Url = url;
			InitializeComponent();
		}

		private int radius = 20;

		[DefaultValue(20)]
		public int Radius
		{
			get { return radius; }
			set
			{
				radius = value;
				this.RecreateRegion();
			}
		}

		private GraphicsPath GetRoundRectagle(Rectangle bounds, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
			path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
			path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius,
				radius, radius, 0, 90);
			path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
			path.CloseAllFigures();
			return path;
		}

		private void RecreateRegion()
		{
			var bounds = ClientRectangle;
			bounds.Width--;
			bounds.Height--;
			using (var path = GetRoundRectagle(bounds, this.Radius))
				this.Region = new Region(path);
			this.Invalidate();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.RecreateRegion();
		}

		private void FavoriteReportBox_Load(object sender, EventArgs e)
		{
			nameSmartLabel.BackColor = SystemColors.ControlLight;
			pictureBox1.Image = Image;
			nameSmartLabel.AddWeightChange().StartLineLimit(3).AddText(ReportName, true).RenderText();
		}

		private void FavoriteReportBox_MouseClick(object sender, MouseEventArgs e)
		{
			log.Debug($"Open link pressed. Url: {Url}");
			RecentUrlQuery.Instance.OpenUrl(Url);
		}
	}
}
