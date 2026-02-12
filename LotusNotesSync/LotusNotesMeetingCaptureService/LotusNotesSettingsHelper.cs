using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using log4net;

namespace LotusNotesMeetingCaptureServiceNamespace
{
	class LotusNotesSettingsHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool IsLotusNotesInstalled
		{
			get { return IsRegistryKeyExists(Registry.ClassesRoot, "Lotus.NotesSession"); }	//This key is not redirected.
		}

		private static bool IsRegistryKeyExists(RegistryKey regkey, string name)
		{
			try
			{
				using (RegistryKey subkey = regkey.OpenSubKey(name))
				{
					return subkey != null;
				}
			}
			catch (Exception ex)
			{
				log.Error(String.Format("Unable to check registry key existence. ({0})", name), ex);
				return false;
			}
		}
	}
}
