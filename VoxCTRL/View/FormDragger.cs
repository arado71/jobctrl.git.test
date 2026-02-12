using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoxCTRL.View
{
	public class FormDragger : IDisposable
	{
		private readonly Form owner;
		private readonly List<Control> draggables = new List<Control>();
		private bool isMouseDown;
		private Point dragPoint;

		public FormDragger(Form ownerFrom)
		{
			owner = ownerFrom;
		}

		public void AddDraggable(Control control)
		{
			control.MouseDown += HandleMouseDown;
			control.MouseUp += HandleMouseUp;
			control.MouseMove += HandleMouseMove;
			draggables.Add(control);
		}
		private void HandleMouseDown(object sender, MouseEventArgs e)
		{
			dragPoint = e.Location;
			isMouseDown = true;
		}

		private void HandleMouseUp(object sender, MouseEventArgs e)
		{
			isMouseDown = false;
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if (isMouseDown)
			{
				// Get the difference between the two points
				int xDiff = dragPoint.X - e.Location.X;
				int yDiff = dragPoint.Y - e.Location.Y;

				// Set the new point
				int x = owner.Location.X - xDiff;
				int y = owner.Location.Y - yDiff;
				owner.Location = new Point(x, y);
			}
		}

		public void Dispose()
		{
			foreach (var control in draggables)
			{
				control.MouseDown -= HandleMouseDown;
				control.MouseUp -= HandleMouseUp;
				control.MouseMove -= HandleMouseMove;
			}
			draggables.Clear();
		}
	}
}
