using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using JcMon2.Data;

namespace JcMon2
{
	public sealed partial class AddNote : Form
	{
		private readonly ControlInfo controlGraph;
		private readonly MainForm parent;

		public AddNote(MainForm parent, ControlInfo controlGraph)
		{
			InitializeComponent();
			this.controlGraph = controlGraph;
			this.parent = parent;
			var window = controlGraph.GetWindowInfo();
			if (window != null)
			{
				Text = window.Title + " - " + window.ProcessName;
				pictureBox1.Image = window.Image;
			}
		}

		private void HandleSaveClicked(object sender, EventArgs e)
		{
			var note = textBox1.Text;
			ThreadPool.QueueUserWorkItem(_ =>
			{
				parent.AddWork();
				var captureData = new CaptureInfo(controlGraph, note) {Type = CaptureType.Manual};
				var formatter = new BinaryFormatter();
				var filename = captureData.GetFileName() + ".jcc";
				using (var ms = new FileStream(filename, FileMode.Create))
				{
					formatter.Serialize(ms, captureData);
				}
				parent.FinishWork();
			});

			Close();
		}

		private void HandleCancelClicked(object sender, EventArgs e)
		{
			Close();
		}
	}
}
