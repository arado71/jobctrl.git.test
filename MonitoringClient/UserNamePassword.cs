using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient
{
	public class UserNamePassword : INotifyPropertyChanged
	{
		private string userName;
		public string UserName
		{
			get { return userName; }
			set
			{
				if (value == userName) return;
				userName = value;
				RaisePropertyChanged("UserName");
				int userId;
				if (!int.TryParse(value, out userId))
				{
					IsValidUser = false;
					throw new ArgumentException("UserId must be an integer");
				}
				IsValidUser = true;
			}
		}

		private string password;
		public string Password
		{
			get { return password; }
			set
			{
				if (value == password) return;
				password = value;
				RaisePropertyChanged("Password");
				if (string.IsNullOrEmpty(value))
				{
					IsValidPass = false;
					throw new ArgumentException("Password cannot be blank");
				}
				IsValidPass = true;
			}
		}

		private bool isValidPass;
		private bool IsValidPass
		{
			get { return isValidPass; }
			set
			{
				isValidPass = value;
				RaisePropertyChanged("IsValid");
			}
		}

		private bool isValidUser;
		private bool IsValidUser
		{
			get { return isValidUser; }
			set
			{
				isValidUser = value;
				RaisePropertyChanged("IsValid");
			}
		}

		public bool IsValid
		{
			get { return IsValidUser && IsValidPass; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propName));
		}
	}
}
