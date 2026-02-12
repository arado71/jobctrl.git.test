using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class CustomMetroTrackBar : MetroTrackBar
	{
		private bool? isTrackBarSliding;

		[Category("Metro Behaviour")]
		public event EventHandler<bool> SlidingChanged;

		public CustomMetroTrackBar()
		{
			ValueChanged += OnValueChanged;
		}

		private void OnValueChanged(object sender, EventArgs e)
		{
			if (isTrackBarSliding.HasValue && !isTrackBarSliding.Value)
			{
				isTrackBarSliding = true;
				SlidingChanged?.Invoke(this, true);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!isTrackBarSliding.HasValue) isTrackBarSliding = false;
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (isTrackBarSliding.HasValue && isTrackBarSliding.Value)
			{
				SlidingChanged?.Invoke(this, false);
			}

			isTrackBarSliding = null;
			base.OnMouseUp(e);
		}

		
	}
}
