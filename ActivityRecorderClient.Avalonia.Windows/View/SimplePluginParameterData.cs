using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.View
{
	public class SimplePluginParameterData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string id;
		public string Id
		{
			get { return id; }
			set { UpdateField(ref id, value, "Id"); }
		}

		private string name;
		public string Name
		{
			get { return name; }
			set { UpdateField(ref name, value, "Name"); }
		}

		private string value;
		public string Value
		{
			get { return value; }
			set { UpdateField(ref this.value, value, "Value"); }
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var propChanged = PropertyChanged;
			if (propChanged != null) propChanged(this, new PropertyChangedEventArgs(propertyName));
		}

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
	}
}
