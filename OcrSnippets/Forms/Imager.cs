using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace OcrConfig.Forms
{
	public partial class Imager : Form
	{
		private Bitmap bmp;

		public Imager(Bitmap _bmp)
		{
			InitializeComponent();
			this.bmp = _bmp;
		}
		public Imager(Bitmap _bmp, string ttl)
			: this(_bmp)
		{
			Text = ttl;
            SendToBack();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetImage();
		}

		private void SetImage()
		{
			if (bmp == null) return;
			if (ClientSize.Width < bmp.Width || ClientSize.Height < bmp.Height + button1.Height)
				this.ClientSize = new Size(bmp.Width, bmp.Height + button1.Height);
			pictureBox1.Image = bmp;
			pictureBox1.Invalidate();
			Application.DoEvents();
		}
		public void SetImage(Bitmap _bmp)
		{
			bmp = _bmp;
			SetImage();
		}

		public void AdjustTitle(int c)
		{
			string[] tt = Text.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
			if (1 < tt.Length)
				Text = tt[0].TrimEnd() + " - " + c;
			else
				Text = Text.TrimEnd() + " - " + c;
		}
		class form
		{
			public Imager iForm { set; get; }
			public int fCounter { set; get; }
		}
		static readonly List<form> forms = new List<form>();

	    public static void Display(Bitmap _bmp, string ttl, bool neew = false)
	    {
	        Imager imager;
	        if (!neew && forms.Any(e => e.iForm.Text.StartsWith(ttl)))
	        {
	            form f = forms.Last(e => e.iForm.Text.StartsWith(ttl));
	            f.iForm.AdjustTitle(++f.fCounter);
	            f.iForm.SetImage(_bmp);
	            imager = f.iForm;
	        }
	        else
	        {
	            imager = new Imager(_bmp, ttl) { StartPosition = FormStartPosition.Manual };
	            if (0 < forms.Count)
	            {
	                imager.Location = new Point(forms[forms.Count - 1].iForm.Left + imager.Width, forms[forms.Count - 1].iForm.Top);
	            }
	            forms.Add(new form() { iForm = imager });
	        }
	        imager.Show();
	    }
	}
}
