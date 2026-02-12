using System;
using System.Drawing;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	sealed partial class Screen : ICaptureEquatable<Screen>
	{
		public Rectangle Bounds
		{
			get { return new Rectangle(X, Y, Width, Height); }
			set
			{
				X = (short)value.X;
				Y = (short)value.Y;
				Width = (short)value.Width;
				Height = (short)value.Height;
			}
		}

		public Bitmap OriginalScreenImage { get; set; }

		public bool CaptureEquals(Screen other)
		{
			if (Object.ReferenceEquals(this, null)) return false;
			if (Object.ReferenceEquals(this, other)) return true;
			return this.X == other.X
				&& this.Y == other.Y
				&& this.Width == other.Width
				&& this.Height == other.Height
				//&& this.Extension == other.Extension //for rule matching this is irrelevant (and we don't care about ScreenShot either)
				&& this.ScreenNumber == other.ScreenNumber
				//we don't care about the ScreenShot
				;
		}
	}
}