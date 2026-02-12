using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class VisibilityTests
	{
		private static void UpdateVisibilityInfo(IList<Screen> screens, IList<DesktopWindow> windows)
		{
			DesktopCaptureService.UpdateVisibilityInfo(screens, windows);
		}

		[Fact]
		public void SimpleWindow()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 } };
			var windows = new[] { new Rectangle(0, 0, 640, 480) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(640 * 480, windows[0].ClientArea);
			Assert.Equal(640 * 480, windows[0].VisibleClientArea);
		}

		[Fact]
		public void OffscreenWindow()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 } };
			var windows = new[] { new Rectangle(640, 0, 640, 480) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(640 * 480, windows[0].ClientArea);
			Assert.Equal(0, windows[0].VisibleClientArea);
		}

		[Fact]
		public void PartialOffscreenWindow()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 } };
			var windows = new[] { new Rectangle(320, 240, 640, 480) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(640 * 480, windows[0].ClientArea);
			Assert.Equal(320 * 240, windows[0].VisibleClientArea);
		}

		[Fact]
		public void SimpleWindowOnTwoMonitors()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 }, new Screen() { X = 640, Y = 120, Width = 640, Height = 240 } };
			var windows = new[] { new Rectangle(0, 120, 1280, 240) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(1280 * 240, windows[0].ClientArea);
			Assert.Equal(1280 * 240, windows[0].VisibleClientArea);
		}

		[Fact]
		public void PartialOffscreenWindowOnTwoMonitors()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 }, new Screen() { X = 640, Y = 120, Width = 640, Height = 240 } };
			var windows = new[] { new Rectangle(-20, -20, 1280, 240) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(1280 * 240, windows[0].ClientArea);
			Assert.Equal(220 * 640 + 100 * 620, windows[0].VisibleClientArea);
		}

		[Fact]
		public void TwoOverlappingWindows()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 } };
			var windows = new[] { new Rectangle(20, 20, 600, 440), new Rectangle(0, 0, 640, 480) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(600 * 440, windows[0].ClientArea);
			Assert.Equal(600 * 440, windows[0].VisibleClientArea);

			Assert.Equal(640 * 480, windows[1].ClientArea);
			Assert.Equal(640 * 480 - 600 * 440, windows[1].VisibleClientArea);
		}

		[Fact]
		public void ThreeOverlappingWindows()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 } };
			var windows = new[] { new Rectangle(40, 40, 560, 400), new Rectangle(20, 20, 600, 440), new Rectangle(0, 0, 640, 480) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(560 * 400, windows[0].ClientArea);
			Assert.Equal(560 * 400, windows[0].VisibleClientArea);

			Assert.Equal(600 * 440, windows[1].ClientArea);
			Assert.Equal(600 * 440 - 560 * 400, windows[1].VisibleClientArea);

			Assert.Equal(640 * 480, windows[2].ClientArea);
			Assert.Equal(640 * 480 - 600 * 440, windows[2].VisibleClientArea);
		}

		[Fact]
		public void ThreePartialOffscreenWindowsOnTwoMonitors()
		{
			//Arrange
			var screens = new List<Screen>() { new Screen() { X = 0, Y = 0, Width = 640, Height = 480 }, new Screen() { X = 640, Y = 120, Width = 640, Height = 240 } };
			var windows = new[] { new Rectangle(5, 10, 25, 50), new Rectangle(0, 0, 15, 20), new Rectangle(-20, -20, 1280, 240) }.Select(n => new DesktopWindow() { ClientRect = n, WindowRect = n }).ToList();

			//Act
			UpdateVisibilityInfo(screens, windows);

			//Assert
			Assert.Equal(25 * 50, windows[0].ClientArea);
			Assert.Equal(25 * 50, windows[0].VisibleClientArea);

			Assert.Equal(15 * 20, windows[1].ClientArea);
			Assert.Equal(15 * 20 - 10 * 10, windows[1].VisibleClientArea);

			Assert.Equal(1280 * 240, windows[2].ClientArea);
			Assert.Equal(220 * 640 + 100 * 620 - 25 * 50 - 15 * 20 + 10 * 10, windows[2].VisibleClientArea);
		}
	}
}
