using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RuleWindowScopeTests
	{
		#region DesktopWindowTests
		[Fact]
		public void DesktopWindowAnyTests()
		{
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
			});
			window.VisibleClientArea = 0;
			window.IsActive = false;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Inactive Hidden");
			window.VisibleClientArea = 1;
			window.IsActive = false;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Inactive Visible");
			window.VisibleClientArea = 0;
			window.IsActive = true;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Active Hidden");
			window.VisibleClientArea = 1;
			window.IsActive = true;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Active Visible");
		}

		[Fact]
		public void DesktopWindowVisibleOrActiveTests()
		{
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.VisibleOrActive,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
			});
			window.VisibleClientArea = 0;
			window.IsActive = false;
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Inactive Hidden");
			window.VisibleClientArea = 1;
			window.IsActive = false;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Inactive Visible");
			window.VisibleClientArea = 0;
			window.IsActive = true;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Active Hidden");
			window.VisibleClientArea = 1;
			window.IsActive = true;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Active Visible");
		}

		[Fact]
		public void DesktopWindowActiveTests()
		{
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
			});
			window.VisibleClientArea = 0;
			window.IsActive = false;
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Inactive Hidden");
			window.VisibleClientArea = 1;
			window.IsActive = false;
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Inactive Visible");
			window.VisibleClientArea = 0;
			window.IsActive = true;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Active Hidden");
			window.VisibleClientArea = 1;
			window.IsActive = true;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window), "Active Visible");
		}
		#endregion

		#region DesktopCaptureTests
		[Fact]
		public void DesktopCaptureAnyTests()
		{
			var desktopCapture = new DesktopCapture()
			{
				DesktopWindows = new List<DesktopWindow>()
				{
					new DesktopWindow() { ProcessName = "a", Title = "Top" },
					new DesktopWindow() { ProcessName = "a", Title = "Bottom" },
				},
			};
			var top = desktopCapture.DesktopWindows[0];
			var bottom = desktopCapture.DesktopWindows[1];

			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "*",
				UrlRule = "*",
				IsRegex = false,
			});

			foreach (WindowState topState in Enum.GetValues(typeof(WindowState)))
			{
				foreach (WindowState bottomState in Enum.GetValues(typeof(WindowState)))
				{
					DesktopWindow matchedWindow;
					SetWindowState(top, topState);
					SetWindowState(bottom, bottomState);
					Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
					Assert.Equal(top, matchedWindow);
				}
			}
		}

		[Fact]
		public void DesktopCaptureVisibleOrActiveTests()
		{
			var desktopCapture = new DesktopCapture()
			{
				DesktopWindows = new List<DesktopWindow>()
				{
					new DesktopWindow() { ProcessName = "a", Title = "Top" },
					new DesktopWindow() { ProcessName = "a", Title = "Bottom" },
				},
			};
			var top = desktopCapture.DesktopWindows[0];
			var bottom = desktopCapture.DesktopWindows[1];

			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.VisibleOrActive,
				ProcessRule = "a",
				TitleRule = "*",
				UrlRule = "*",
				IsRegex = false,
			});

			foreach (WindowState topState in Enum.GetValues(typeof(WindowState)))
			{
				foreach (WindowState bottomState in Enum.GetValues(typeof(WindowState)))
				{
					DesktopWindow matchedWindow;
					SetWindowState(top, topState);
					SetWindowState(bottom, bottomState);
					if (topState == WindowState.InactiveHidden && bottomState == WindowState.InactiveHidden) //no match
					{
						Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
					}
					else if (topState == WindowState.InactiveHidden) //bottom only match
					{
						Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
						Assert.Equal(bottom, matchedWindow);
					}
					else
					{
						Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
						Assert.Equal(top, matchedWindow);
					}
				}
			}
		}

		[Fact]
		public void DesktopCaptureActiveTests()
		{
			var desktopCapture = new DesktopCapture()
			{
				DesktopWindows = new List<DesktopWindow>()
				{
					new DesktopWindow() { ProcessName = "a", Title = "Top" },
					new DesktopWindow() { ProcessName = "a", Title = "Bottom" },
				},
			};
			var top = desktopCapture.DesktopWindows[0];
			var bottom = desktopCapture.DesktopWindows[1];

			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "a",
				TitleRule = "*",
				UrlRule = "*",
				IsRegex = false,
			});

			foreach (WindowState topState in Enum.GetValues(typeof(WindowState)))
			{
				foreach (WindowState bottomState in Enum.GetValues(typeof(WindowState)))
				{
					DesktopWindow matchedWindow;
					SetWindowState(top, topState);
					SetWindowState(bottom, bottomState);
					if ((topState == WindowState.InactiveHidden || topState == WindowState.InactiveVisible)
						&& (bottomState == WindowState.InactiveHidden || bottomState == WindowState.InactiveVisible)) //no match
					{
						Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
					}
					else if ((topState == WindowState.InactiveHidden || topState == WindowState.InactiveVisible)) //bottom only match
					{
						Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
						Assert.Equal(bottom, matchedWindow);
					}
					else
					{
						Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow), "Top " + topState + " Bottom " + bottomState);
						Assert.Equal(top, matchedWindow);
					}
				}
			}
		}
		#endregion

		private static void SetWindowState(DesktopWindow window, WindowState state)
		{
			Assert.NotNull(window);
			switch (state)
			{
				case WindowState.InactiveHidden:
					window.IsActive = false;
					window.VisibleClientArea = 0;
					break;
				case WindowState.InactiveVisible:
					window.IsActive = false;
					window.VisibleClientArea = 1;
					break;
				case WindowState.ActiveHidden:
					window.IsActive = true;
					window.VisibleClientArea = 0;
					break;
				case WindowState.ActiveVisible:
					window.IsActive = true;
					window.VisibleClientArea = 1;
					break;
				default:
					throw new ArgumentOutOfRangeException("state");
			}
		}

		private enum WindowState
		{
			InactiveHidden,
			InactiveVisible,
			ActiveHidden,
			ActiveVisible,
		}
	}
}
