using System;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class FavoriteButton : UserControl
	{
		private bool isFavorite = false;
		private bool isHovering = false;

		public bool IsFavorite
		{
			get { return isFavorite; }

			set
			{
				isFavorite = value;
				BackgroundImage = value ? Resources.favorite_on : (isHovering ? Resources.favorite_off : Resources.favorite);
			}
		}

		public FavoriteButton()
		{
			InitializeComponent();
		}

		private void HandleMouseEntered(object sender, EventArgs e)
		{
			isHovering = true;
			BackgroundImage = !isFavorite ? Resources.favorite_off : Resources.favorite_on;
		}

		private void HandleMouseLeft(object sender, EventArgs e)
		{
			isHovering = false;
			BackgroundImage = !isFavorite ? Resources.favorite : Resources.favorite_on;
		}
	}
}