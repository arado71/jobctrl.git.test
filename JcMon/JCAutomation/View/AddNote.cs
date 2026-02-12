using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using JCAutomation.Data;

namespace JCAutomation.View
{
	public sealed partial class AddNote : Form
	{
		private readonly ControlInfo controlGraph;
		private readonly TouchForm parent;

		public AddNote(TouchForm parent, ControlInfo controlGraph)
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
