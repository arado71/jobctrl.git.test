using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JC.Removal.Utils
{
	class IniFile   // revision 11
	{
		private readonly string path;

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

		public IniFile(string iniPath)
		{
			path = new FileInfo(iniPath).FullName;
		}

		public string Read(string key, string section)
		{
			var retVal = new StringBuilder(255);
			GetPrivateProfileString(section, key, "", retVal, 255, path);
			return retVal.ToString();
		}

		public void Write(string key, string value, string section)
		{
			WritePrivateProfileString(section, key, value, path);
		}

		public void DeleteKey(string key, string section)
		{
			Write(key, null, section);
		}

		public void DeleteSection(string section)
		{
			Write(null, null, section);
		}

		public bool KeyExists(string key, string section)
		{
			return Read(key, section).Length > 0;
		}
	}
}
