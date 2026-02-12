using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Tct.ActivityRecorderService.Maintenance;

namespace Tct.ActivityRecorderService.ClientComputerData
{
	[System.Reflection.Obfuscation(Exclude = false, ApplyToMembers = true)]
	public class ClientComputerErrorPath : IFileCleanup
	{
		private static ClientComputerErrorPath instance = null;
		private static readonly object creationLock = new object();

		public static ClientComputerErrorPath Instance
		{
			get
			{
				if (instance == null)
				{
					lock (creationLock)
					{
						if (instance == null)
						{
							instance = new ClientComputerErrorPath();
						}
					}
				}

				return instance;
			}
		}

		public Maintenance.Storage Type
		{
			get
			{
				return Maintenance.Storage.Log;
			}
		}

		private ClientComputerErrorPath()
		{
		}

		public void Save(ClientComputerError clientError)
		{
			var path = GetPath(clientError);
			File.WriteAllBytes(path, clientError.Data);
		}

		public string GetPath(ClientComputerError clientError)
		{
			string dir, fileName;
			GetPath(clientError, ConfigManager.ClientLogsDir, out dir, out fileName);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			return Path.Combine(dir, fileName);
		}

		public ILookup<int, int> GetUserIds()
		{
			var result = new List<Tuple<int, int>>();
			foreach (var dir in Directory.EnumerateDirectories(ConfigManager.ClientLogsDir))
			{
				if (!int.TryParse(Path.GetFileName(dir), out var companyId)) continue;
				foreach (var filePart in Directory.EnumerateFiles(dir).Where(d => Path.GetFileName(d).Contains("_")).Select(d => Path.GetFileName(d).Split('_')[0]).Distinct())
				{
					if (!int.TryParse(filePart, out var userId)) continue;
					result.Add(Tuple.Create(companyId, userId));
				}
			}

			return result.ToLookup(k => k.Item1, v => v.Item2);
		}

		public IEnumerable<string> GetPaths(int companyId, int? userId)
		{
			return new[] { Path.Combine(ConfigManager.ClientLogsDir, companyId.ToString(CultureInfo.InvariantCulture)) };
		}

		public string GetUrl(ClientComputerError clientError)
		{
			string dir, fileName;
			GetPath(clientError, ConfigManager.ClientLogsUrl, out dir, out fileName);
			return Path.Combine(dir, fileName).Replace("\\", "/");	//Replace is an ugly hax. Anyway this may be changing according to IIS settings. Security???
		}

		private void GetPath(ClientComputerError clientError, string root, out string dir, out string fileName)
		{
			if (clientError == null) throw new ArgumentNullException("clientError");

			fileName = clientError.UserId
				+ "_" + clientError.FirstReceiveDate.ToString("HH-mm-ss")
				+ "_" + clientError.Id
				+ ".zip";

			var subdirs = Path.Combine(root, clientError.CompanyId.ToString());
			subdirs = Path.Combine(subdirs, clientError.UserId.ToString());
			subdirs = Path.Combine(subdirs, clientError.FirstReceiveDate.ToString("yyyy-MM-dd"));
			dir = subdirs;
		}
	}
}
