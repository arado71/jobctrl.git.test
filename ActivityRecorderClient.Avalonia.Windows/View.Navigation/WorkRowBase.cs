using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class WorkRowBase : SelectableControl<NavigationBase>, ILocalizableControl
	{
		protected NavigationBase navigation;

		public NavigationBase Navigation
		{
			get { return navigation; }

			set
			{
				if (navigation == value) return;
				Debug.Assert(value != null);
				var oldVal = navigation;
				if (oldVal != null)
				{
					oldVal.PropertyChanged -= HandleNavigationChanged;
				}

				navigation = value;
				Value = value;
				navigation.PropertyChanged += HandleNavigationChanged;
				navigation.SimulateChange(oldVal, HandleNavigationChanged);
			}
		}

		public virtual void Localize()
		{
			navigation?.Localize();
		}

		public virtual void SetColorScheme()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (navigation != null)
			{
				navigation.PropertyChanged -= HandleNavigationChanged;
				navigation = null;
			}

 			base.Dispose(disposing);
		}

		protected WorkRowBase()
		{
			DoubleBuffered = true;
		}

		protected virtual void OnCanFavoriteChanged()
		{
		}

		protected virtual void OnEndDateChanged()
		{
		}

		protected virtual void OnIconChanged()
		{
		}

		protected virtual void OnIdChanged()
		{
		}

		protected virtual void OnIsFavoriteChanged()
		{
		}

		protected virtual void OnIsWorkChanged()
		{
		}

		protected virtual void OnNameChanged()
		{
		}

		protected virtual void OnPathChanged()
		{
		}

		protected virtual void OnPriorityChanged()
		{
		}

		protected virtual void OnRemainingTimeChanged()
		{
		}

		protected virtual void OnStartDateChanged()
		{
		}

		protected virtual void OnTotalTimeChanged()
		{
		}

		protected virtual void OnUsedTimeChanged()
		{
		}

		protected virtual void OnIsEditableChanged()
		{
		}

		private void HandleNavigationChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Id":
					OnIdChanged();
					break;
				case "Name":
					OnNameChanged();
					break;
				case "Path":
					OnPathChanged();
					break;
				case "Icon":
					OnIconChanged();
					break;
				case "CanFavorite":
					OnCanFavoriteChanged();
					break;
				case "IsFavorite":
					OnIsFavoriteChanged();
					break;
				case "TotalTime":
					OnTotalTimeChanged();
					break;
				case "UsedTime":
					OnUsedTimeChanged();
					break;
				case "RemainingTime":
					OnRemainingTimeChanged();
					break;
				case "IsWork":
					OnIsWorkChanged();
					break;
				case "StartDate":
					OnStartDateChanged();
					break;
				case "Priority":
					OnPriorityChanged();
					break;
				case "EndDate":
					OnEndDateChanged();
					break;
				case "IsEditable":
					OnIsEditableChanged();
					break;
				default:
					break;
			}
		}
	}
}