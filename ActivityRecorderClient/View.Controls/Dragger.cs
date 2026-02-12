using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed class Dragger : IDisposable
	{
		private const float DragOpacity = 0.8f;
		private bool disposed = false;
		public static Control Dragged { get; private set; }
		public static Cursor DragCursor { get; private set; }
		public static Cursor DragNoCursor { get; private set; }

		public Dragger(Control c, Point? position = null)
		{
			Dragged = c;
			Point offset = position ?? c.PointToClient(Cursor.Position);
			using (Bitmap bm = CursorHelper.AsBitmap(c))
			{
				DragCursor = CursorHelper.CreateCursor(bm, Cursors.SizeAll, offset, DragOpacity);
				DragNoCursor = CursorHelper.CreateCursor(bm, Cursors.No, offset, DragOpacity);
			}
		}

		// This gets called once when we move over a new control, or continuously if that control supports dropping.

		public void Dispose()
		{
			if (disposed) return;

			Cleanup();
			disposed = true;
		}

		public void UpdateCursor(object sender, GiveFeedbackEventArgs fea)
		{
			fea.UseDefaultCursors = false;
			switch (fea.Effect)
			{
				case DragDropEffects.Copy:
					Cursor.Current = DragCursor;
					break;
				case DragDropEffects.Move:
					Cursor.Current = DragCursor;
					break;
				default:
					Cursor.Current = DragNoCursor;
					break;
			}
		}

		private void Cleanup()
		{
			WinApi.DestroyIcon(DragCursor.Handle);
			DragCursor.Dispose();
			DragCursor = null;
			WinApi.DestroyIcon(DragNoCursor.Handle);
			DragNoCursor.Dispose();
			DragNoCursor = null;
		}
	}
}