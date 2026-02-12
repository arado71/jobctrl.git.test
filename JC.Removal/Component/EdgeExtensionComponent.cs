using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace JC.Removal.Component
{
	class EdgeExtensionComponent: BaseComponent
	{
		private const string extensionFileName = "edgeExtension.appx";
		private static readonly string SubDirName = "EdgeInterop";
		private static string AppName = "com.jobctrl.jobctrl";
		private const string userFriendlyName = "Edge extension";

		public override bool Remove(out string error)
		{
			throw new NotImplementedException();
			//using (var powerShellInstance = System.Management.Automation.PowerShell.Create())
			//{
			//	string script = $"Remove-AppxPackage -Name {AppName}";
			//	powerShellInstance.AddScript(script);
			//	powerShellInstance.Invoke();
			//	if (powerShellInstance.Streams.Error.Count > 0)
			//	{
			//		error = string.Join<ErrorRecord>(Environment.NewLine, powerShellInstance.Streams.Error.ToArray());
			//		return false;
			//	}
			//}
			//error = null;
			//return true;
		}

		public override bool IsInstalled()
		{
			throw new NotImplementedException();

			//using (var powerShellInstance = PowerShell.Create())
			//{
			//	string script = $"Get-AppxPackage -Name {AppName}";
			//	powerShellInstance.AddScript(script);
			//	var result = powerShellInstance.Invoke();
			//	return result.Count > 0;
			//}
		}

		public override string[] GetProcessesNames()
		{
			return new string[0];
		}

		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}
	}
}
