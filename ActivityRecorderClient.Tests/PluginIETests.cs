using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class PluginIETests
	{
		[Fact]
		public void GenerateSettingsJcMenu()
		{
			var sett = new List<DomSettings>();
			sett.Add(new DomSettings() { Key = "JCP", PropertyName = "innerText", Selector = "td.MenuItem.Selected", UrlPattern = "jobctrl\\.com" });
			sett.Add(new DomSettings() { Key = "JC", PropertyName = "innerText", Selector = "#form1 table.p tbody tr td table.page tbody tr td table tbody tr td table tbody tr td div table tbody tr td div p span", UrlPattern = "jobctrl\\.com" });
			Console.WriteLine(JsonHelper.SerializeData(sett));
		}

		[Fact]
		public void GenerateSettingsMinimus()
		{
			var sett = new List<DomSettings>();
			sett.Add(new DomSettings() { Key = "MiniCim", EvalString = "document.getElementById('_COMP_COMP_FG_KATE').innerText", UrlPattern = "https?://minimus/CompWeb2/FormView/FormView.aspx" });
			sett.Add(new DomSettings() { Key = "MiniAlCim", EvalString = "document.getElementById('_COMP_COMP_FG_TYPE').innerText", UrlPattern = "https?://minimus/CompWeb2/FormView/FormView.aspx" });
			sett.Add(new DomSettings() { Key = "MiniFel", EvalString = "jc_res = null;for(i=0;i<5;i++){var elem = document.getElementById('_COMP_COMP_ID_ID_Comp_ID_Azonositas_'+i);if (elem && elem.checked) { jc_res = elem.value; elem = document.getElementById('_COMP_COMP_ID_ID_Comp_ID_'+elem.value); if (elem) jc_res = jc_res + ' ' + elem.value ;}}", UrlPattern = "https?://minimus/CompWeb2/FormView/FormView.aspx" });
			Console.WriteLine(JsonHelper.SerializeData(sett));
		}
	}
}
