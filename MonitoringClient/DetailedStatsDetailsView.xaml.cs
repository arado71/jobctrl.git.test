using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient
{
	public partial class DetailedStatsDetailsView : UserControl
	{
		public DetailedStatsDetailsView()
		{
			InitializeComponent();
		}

		private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
		{
			Canvas.SetZIndex((UIElement)((FrameworkElement)sender).Parent, 1); //I don't know how to set this in the Style :(
		}

		private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
		{
			Canvas.SetZIndex((UIElement)((FrameworkElement)sender).Parent, 0);
		}

		private void mainContent_MouseEnter(object sender, MouseEventArgs e)
		{
			var ctrl = (Viewbox)sender;
			var root = VisualTreeHelperEx.GetParentOfType<WrapPanel>(ctrl);
			//var root = Application.Current.RootVisual;
			if (root == null) return;
			var rootSize = root.RenderSize;
			var gt = ctrl.TransformToVisual(root);
			var offset = gt.Transform(new Point(0, 0)); //get the top left corner Point

			var xOrig = rootSize.Width > ctrl.ActualWidth ? offset.X / (rootSize.Width - ctrl.ActualWidth) : 0.5;
			var yOrig = rootSize.Height > ctrl.ActualHeight ? offset.Y / (rootSize.Height - ctrl.ActualHeight) : 0.5;

			xOrig = Math.Max(Math.Min(1, xOrig), 0); //clamp values to 0..1
			yOrig = Math.Max(Math.Min(1, yOrig), 0);

			ctrl.RenderTransformOrigin = new Point(xOrig, yOrig);
		}
	}
}
