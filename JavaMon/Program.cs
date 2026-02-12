using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JavaMon;
using Tct.ActivityRecorderClient.Hotkeys;

namespace JavaMon
{
	static class Program
	{
		public static readonly HotkeyWinService HotkeyService = new HotkeyWinService();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			log4net.Config.XmlConfigurator.Configure();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.AddMessageFilter(HotkeyService);
			Application.Run(new JavaAccessibilityForm());
			Application.RemoveMessageFilter(HotkeyService);
		}
	}
}
