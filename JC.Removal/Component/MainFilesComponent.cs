using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JC.Removal.Component
{
	class MainFilesComponent: BaseComponent
	{
		private readonly string jcExePath;
		private const string userFriendlyName = "Files";

		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}

		public MainFilesComponent(string jcExePath)
		{
			this.jcExePath = jcExePath;
		}

		public override bool Remove(out string error)
		{
			try
			{
				var dir = Path.GetDirectoryName(jcExePath);
				Directory.Delete(dir, true);
				error = null;
				return true;
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				return false;
			}
		}

		public override bool IsInstalled()
		{
			return File.Exists(jcExePath);
		}

		public override string[] GetProcessesNames()
		{
			var dir = Path.GetDirectoryName(jcExePath);
			return Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories).ToArray();
		}
	}
}
