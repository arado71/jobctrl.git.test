using System;
using System.Runtime.InteropServices;
using WindowsInstaller;
using log4net;

namespace Tct.ActivityRecorderService.Update
{
	public static class InstallerHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static string GetMsiProperty(string msiFile, string property)
		{
			var retVal = string.Empty;
			object installer = null;
			Database database = null;
			View view = null;
			Record record = null;
			try
			{
				// Create an Installer instance
				var classType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
				installer = Activator.CreateInstance(classType);

				// Open the msi file for reading
				// 0 - Read, 1 - Read/Write
				database = ((Installer)installer).OpenDatabase(msiFile, 0);

				// Fetch the requested property
				var sql = String.Format("SELECT Value FROM Property WHERE Property ='{0}'", property);
				view = database.OpenView(sql);
				view.Execute(null);

				// Read in the fetched record
				record = view.Fetch();
				if (record != null) retVal = record.get_StringData(1);

				view.Close();

				return retVal;
			}
			catch (Exception ex)
			{
				log.Error("Error reading msi property " + property + " from file " + msiFile, ex);
				return retVal;
			}
			finally
			{
				if (record != null) Marshal.ReleaseComObject(record);
				if (view != null) Marshal.ReleaseComObject(view);
				if (database != null) Marshal.ReleaseComObject(database);
				if (installer != null) Marshal.ReleaseComObject(installer);
			}
		}
	}
}
