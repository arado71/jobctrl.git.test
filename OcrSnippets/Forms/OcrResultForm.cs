using System;
using System.Drawing;
using System.Windows.Forms;

namespace TcT.OcrSnippets
{
    public partial class OcrResultForm : Form
    {
        public OcrResultForm()
        {
            InitializeComponent();
        }

        public static void ShowOcr(OcrResult result)
        {
            var form = new OcrResultForm();
            if (!result.Success)
            {
                form.txtLog.Text = "ERROR: " + result.Error;
                form.txtLog.ForeColor = Color.Red;
            }
            else
            {
                form.txtLog.Text = result.Text.Replace("\n", Environment.NewLine);
                form.txtLog.ForeColor = SystemColors.WindowText;
            }
            form.txtLog.Select(0, 0);
            form.Show();
        }
    }
}
