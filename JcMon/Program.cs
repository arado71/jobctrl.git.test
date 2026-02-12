using JCAutomation.SystemAdapter;
using JCAutomation.View;

namespace JCAutomation
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;
	using Tct.ActivityRecorderClient;
	using Tct.ActivityRecorderClient.Hotkeys;

    internal static class Program
    {
        public static readonly HotkeyWinService HotkeyService = new HotkeyWinService();

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowErrorDialog(e.Exception, "(Main)", true);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Split(new char[] { ',' })[0] + ".dll";
            string name = (from n in Assembly.GetExecutingAssembly().GetManifestResourceNames()
                where n.EndsWith(dllName)
                select n).FirstOrDefault<string>();
            if (name == null)
            {
                return null;
            }
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                return Assembly.Load(StreamToBytes(stream));
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = e.ExceptionObject as Exception;
            ShowErrorDialog(exceptionObject, "(Domain)", false);
            Environment.Exit(-1);
        }

        [STAThread]
        private static void Main()
        {
			WinApi.ScreenReaderOn();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Program.CurrentDomain_AssemblyResolve);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Program.Application_ThreadException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(HotkeyService);
            Application.Run(new MainForm());
            Application.RemoveMessageFilter(HotkeyService);
			WinApi.ScreenReaderOff();
        }

        private static void ShowErrorDialog(Exception ex, string addInfo, bool canContinue)
        {
            using (ThreadExceptionDialog dialog = new ThreadExceptionDialog(ex ?? new Exception("Unknown Exception")))
            {
                if (!canContinue)
                {
                    ((Button) dialog.CancelButton).Enabled = false;
                }
                dialog.Text = dialog.Text + " " + addInfo + ", Please send details to support";
                if (dialog.ShowDialog() == DialogResult.Abort)
                {
                    Application.Exit();
                }
            }
        }

        private static byte[] StreamToBytes(Stream input)
        {
            int capacity = input.CanSeek ? ((int) input.Length) : 0;
            using (MemoryStream stream = new MemoryStream(capacity))
            {
                int num2;
                byte[] buffer = new byte[0x1000];
                do
                {
                    num2 = input.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, num2);
                }
                while (num2 != 0);
                return stream.ToArray();
            }
        }
    }
}

