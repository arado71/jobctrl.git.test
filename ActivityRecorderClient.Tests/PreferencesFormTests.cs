using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Controls;
using Tct.ActivityRecorderClient.View;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class PreferencesFormTests
	{
		[Fact]
		public void TabIndexIsZero()
		{
			using (var form = new PreferencesForm())
			{
				var tabs = GetDescendants(form).OfType<MetroTabControl>().ToList();
				Assert.True(tabs.Count > 0);
				Assert.True(tabs.All(n => n.SelectedIndex == 0));
			}
		}

		private static IEnumerable<Control> GetDescendants(Control parent)
		{
			foreach (Control control in parent.Controls)
			{
				yield return control;
				foreach (var child in GetDescendants(control))
				{
					yield return control;
				}
			}
		}
	}
}
