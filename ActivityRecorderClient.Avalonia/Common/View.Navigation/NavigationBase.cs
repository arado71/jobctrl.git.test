using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public abstract class NavigationBase : IEquatable<NavigationBase>, INotifyPropertyChanged, IDisposable, ILocalizableControl
	{
		public enum RenderHint
		{
			Short,
			Long,
			Progress,
			Remaining,
			Priority
		}

		protected const int MenuMaxSize = 50;
		private readonly INavigator navigator;
		private readonly LocationKey key; //as I understand NavigationBase is tied to a LocationKey
		private bool canFavorite;
		private LocationKey[] children;
		private bool disposed;
		private DateTime? endDate;
		private Image icon;

		private int id;
		protected bool isFavorite;
		private bool isWork;
		protected bool isEditable;
		private string name = string.Empty;
		//private LocationKey parent = null;
		private IEnumerable<string> path = null;
		private int? priority;
		private TimeSpan? remainingTime;

		// Parent property
		private RenderHint render;
		private bool reorderable = false;
		private DateTime? startDate;
		private TimeSpan? totalTime;
		private TimeSpan usedTime;

		protected INavigator Navigator { get { return navigator; } }

		public LocationKey Key { get { return key; } }

		public int Id
		{
			get { return id; }
			private set { UpdateField(ref id, value, "Id"); }
		}

		public RenderHint Render
		{
			get { return render; }
			protected set { UpdateField(ref render, value, "Render"); }
		}

		// Parent property

		//public LocationKey Parent
		//{
		//	get { return parent; }
		//	set { UpdateField(ref parent, value, "Parent"); }
		//}

		public IEnumerable<string> Path
		{
			get { return path; }
			set
			{
				if (ReferenceEquals(path, value) ||
				    (path != null && value != null && path.SequenceEqual(value)))
				{
					return;
				}

				path = value;
				OnPropertyChanged("Path");
			}
		}

#pragma warning disable CA1416
		public Image Icon
		{
			get { return icon; }
			set { UpdateField(ref icon, value, "Icon"); }
		}
#pragma warning restore CA1416


		public string Name
		{
			get { return name; }
			set { UpdateField(ref name, value, "Name"); }
		}

		// Parent property

		public bool Reorderable
		{
			get { return reorderable; }
			set { UpdateField(ref reorderable, value, "Reorderable"); }
		}

		public bool IsEditable
		{
			get { return isEditable; }
			set { UpdateField(ref isEditable, value, "IsEditable"); }
		}

		public TimeSpan? TotalTime
		{
			get { return totalTime; }
			set { UpdateField(ref totalTime, value, "TotalTime"); }
		}

		public TimeSpan UsedTime
		{
			get { return usedTime; }
			set { UpdateField(ref usedTime, value, "UsedTime"); }
		}

		public TimeSpan? RemainingTime
		{
			get { return remainingTime; }
			set { UpdateField(ref remainingTime, value, "RemainingTime"); }
		}

		public bool IsWork
		{
			get { return isWork; }
			set { UpdateField(ref isWork, value, "IsWork"); }
		}


		public DateTime? StartDate
		{
			get { return startDate; }
			set { UpdateField(ref startDate, value, "StartDate"); }
		}

		public int? Priority
		{
			get { return priority; }
			set { UpdateField(ref priority, value, "Priority"); }
		}

		public DateTime? EndDate
		{
			get { return endDate; }
			set { UpdateField(ref endDate, value, "EndDate"); }
		}

		public bool IsFavorite
		{
			get { return isFavorite; }
			set
			{
				if (isFavorite == value) return;
				FavoritesHelper.ToggleFavorite(Id);
				UpdateField(ref isFavorite, value, "IsFavorite");
			}
		}

		public bool CanFavorite
		{
			get { return canFavorite; }
			set { UpdateField(ref canFavorite, value, "CanFavorite"); }
		}

		public LocationKey[] Children
		{
			get
			{
				if (children != null) return children;
				return (children = GetChildren() ?? new LocationKey[0]);
			}
			protected set
			{
				if (ReferenceEquals(children, value) ||
					(children != null && value != null && children.SequenceEqual(value)))
				{
					return;
				}

				children = value;
				OnPropertyChanged("Children");
			}
		}

		protected NavigationBase(LocationKey reference, INavigator navigator)
		{
			key = reference;
			this.navigator = navigator;
			Path = null;
			Id = key.Id;
			LocalizeBase();
		}

		private void LocalizeBase()
		{ 
			// calling virtual function
			Localize();
		}

		public void Dispose()
		{
			if (disposed) return;
			Dispose(true);
			disposed = true;
		}

		public bool Equals(NavigationBase other)
		{
			// type difference not stored, hence not checked
			return other != null && other.Id == Id;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void Navigate(bool leaveTrail = true)
		{
			Navigator.Goto(key, leaveTrail);
		}

		// Parent method
		public virtual void Reorder(NavigationBase child, int position)
		{
		}

		public void SimulateChange(NavigationBase oldNavigation, PropertyChangedEventHandler eventHandler)
		{
			if (eventHandler == null) return;
			if (oldNavigation == null || oldNavigation.Id != Id) eventHandler(this, new PropertyChangedEventArgs("Id"));
			if (oldNavigation == null || oldNavigation.Name != Name) eventHandler(this, new PropertyChangedEventArgs("Name"));
			if (oldNavigation == null || (Path == null ^ oldNavigation.Path == null) || (!ReferenceEquals(Path, oldNavigation.Path) && !oldNavigation.Path.SequenceEqual(Path)))
				eventHandler(this, new PropertyChangedEventArgs("Path"));
			if (oldNavigation == null || oldNavigation.Icon != Icon) eventHandler(this, new PropertyChangedEventArgs("Icon"));
			if (oldNavigation == null || oldNavigation.IsFavorite != IsFavorite)
				eventHandler(this, new PropertyChangedEventArgs("IsFavorite"));
			if (oldNavigation == null || oldNavigation.IsEditable != IsEditable)
				eventHandler(this, new PropertyChangedEventArgs("IsEditable"));
			if (oldNavigation == null || oldNavigation.Reorderable != Reorderable)
				eventHandler(this, new PropertyChangedEventArgs("Reorderable"));
			if (oldNavigation == null || oldNavigation.Render != Render)
				eventHandler(this, new PropertyChangedEventArgs("Render"));
			if (oldNavigation == null || oldNavigation.TotalTime != TotalTime)
				eventHandler(this, new PropertyChangedEventArgs("TotalTime"));
			if (oldNavigation == null || oldNavigation.UsedTime != UsedTime)
				eventHandler(this, new PropertyChangedEventArgs("UsedTime"));
			if (oldNavigation == null || oldNavigation.RemainingTime != RemainingTime)
				eventHandler(this, new PropertyChangedEventArgs("RemainingTime"));
			if (oldNavigation == null || oldNavigation.IsWork != IsWork)
				eventHandler(this, new PropertyChangedEventArgs("IsWork"));
			if (oldNavigation == null || oldNavigation.StartDate != StartDate)
				eventHandler(this, new PropertyChangedEventArgs("StartDate"));
			if (oldNavigation == null || oldNavigation.Priority != Priority)
				eventHandler(this, new PropertyChangedEventArgs("Priority"));
			if (oldNavigation == null || oldNavigation.EndDate != EndDate)
				eventHandler(this, new PropertyChangedEventArgs("EndDate"));
			if (oldNavigation == null || Children == null || oldNavigation.Children == null ||
				!oldNavigation.Children.SequenceEqual(Children)) eventHandler(this, new PropertyChangedEventArgs("Children"));
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var propChanged = PropertyChanged;
			if (propChanged != null) propChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected abstract LocationKey[] GetChildren();

		protected bool UpdateField<T>(ref T field, T value, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
				OnPropertyChanged(propertyName);
				return true;
			}

			return false;
		}

		public abstract void Localize();
	}
}