using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient
{
	public partial class LoginWindow : ChildWindow
	{
		public UserNamePassword UserNamePassword { get; set; }

		public LoginWindow()
		{
			InitializeComponent();
			UserNamePassword = new UserNamePassword();
			this.DataContext = UserNamePassword;
			this.Title = "Login - JobCTRL.com Online Monitoring v" + (new AssemblyName(Assembly.GetExecutingAssembly().FullName)).Version.ToString(3);
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void txtUserId_TextChanged(object sender, TextChangedEventArgs e)
		{
			BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
			if (binding != null)
			{
				binding.UpdateSource();
			}
		}

		private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			BindingExpression binding = ((PasswordBox)sender).GetBindingExpression(PasswordBox.PasswordProperty);
			if (binding != null)
			{
				binding.UpdateSource();
			}
		}

		private void txtPassword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && UserNamePassword.IsValid)
			{
				this.DialogResult = true;
			}
		}

		private void txtUserId_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (UserNamePassword.IsValid)
				{
					this.DialogResult = true;
				}
				else
				{
					txtPassword.Focus();
				}
			}
		}
	}
}

