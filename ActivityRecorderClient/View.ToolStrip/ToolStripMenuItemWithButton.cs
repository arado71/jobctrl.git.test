using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class ToolStripMenuItemWithButton : ToolStripMenuItem
	{
		public new event MouseEventHandler MouseDown;
		public event EventHandler ButtonClick;
		public char ButtonChar { get; set; }
		public char AltButtonChar { get; set; }
		public bool IsButtonVisible { get; set; }
		public virtual WorkData WorkData { get; set; }

		public ToolStripMenuItemWithButton()
		{
			Init();
		}

		public ToolStripMenuItemWithButton(string text)
			: base(text)
		{
			Init();
		}

		public Rectangle ButtonRectangle
		{
			get
			{
				var size = Math.Min(ContentRectangle.Height, ContentRectangle.Width);
				var contRect = ContentRectangle;
				return new Rectangle(contRect.X + contRect.Width - size, contRect.Y, size, size);
			}
		}

		private void Init()
		{
			IsButtonVisible = false;
			ButtonChar = '=';
			AltButtonChar = 'A';
		}

		private bool IsMouseOverButton()
		{
			if (!IsButtonVisible) return false;
			var pos = Cursor.Position;
			var clientPos = Owner.PointToClient(pos);
			clientPos.Offset(new Point(-Bounds.Location.X, -Bounds.Location.Y));
			return ButtonRectangle.Contains(clientPos);
		}

		private bool isMousePressed;
		protected override void OnClick(EventArgs e)
		{
			if (IsMouseOverButton())
			{
				OnButtonClick(e);
			}
			else
			{
				if (!isMousePressed) // omit clicks after mousedown
					base.OnClick(e);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (IsMouseOverButton())
			{
				OnButtonClick(EventArgs.Empty);
				//mouse events would still be raised (despite not calling base.OnMouseDown(e)) that is why we must have a new MouseDown
			}
			else
			{
				isMousePressed = true;
				RaiseMouseDown(e);
				base.OnMouseDown(e);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			isMousePressed = false;
			base.OnMouseUp(e);
		}

		private void RaiseMouseDown(MouseEventArgs e)
		{
			var del = this.MouseDown;
			if (del != null) del(this, e);
		}

		protected virtual void OnButtonClick(EventArgs e)
		{
			var del = ButtonClick;
			if (del != null) del(this, e);
		}
	}
}
