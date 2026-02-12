using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.Capturing.Meeting.LotusNotes;

namespace Tct.ActivityRecorderClient.View
{
	public partial class LotusNotesLoginForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public string UserPassword { get; private set; }
		public bool RememberMe { get; private set; }

		public LotusNotesLoginForm()
		{
			InitializeComponent();
			this.SetFormStartPositionCenterScreen();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			lblPassword.Text = Labels.Login_Password + " (Lotus Notes)";
			cbRememberMe.Text = Labels.Login_RememeberMe;
			btnOk.Text = Labels.Ok;
		}

		private void CenterHorizontally(Control control)
		{
			control.Left = (ClientSize.Width - control.Width) / 2;
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			if (!(sender is Control)) return;
			CenterHorizontally(sender as Control);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			UserPassword = txtPassword.Text;
			RememberMe = cbRememberMe.Checked;
			DialogResult = String.IsNullOrEmpty(txtPassword.Text) ? DialogResult.Cancel : DialogResult.OK;
		}

		private void txtPassword_TextChanged(object sender, EventArgs e)
		{
			btnOk.Enabled = !String.IsNullOrEmpty(txtPassword.Text);
		}

		private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return) btnOk_Click(sender, e);
		}

		private void LotusNotesLoginForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.Cancel) return;

			var mbres = MessageBox.Show(this, Labels.LoginToLotusNotes_NotificationAboutCancellingLoginBody, Labels.LoginToLotusNotes_NotificationAboutCancellingLoginTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
			if (mbres == DialogResult.Cancel) e.Cancel = true;
		}

		public static LotusNotesMeetingCaptureService.LNLoginData DisplayLoginForm()
		{
			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			{
				return DisplayLoginFormImpl();
			}
			return STAThreadRunner<LotusNotesMeetingCaptureService.LNLoginData>.CallOnSTA(DisplayLoginFormImpl);
		}

		private static LotusNotesMeetingCaptureService.LNLoginData DisplayLoginFormImpl()
		{
			using (var loginForm = new LotusNotesLoginForm())
			{
				loginForm.ShowDialog();
				log.Info("Lotus Notes login result " + loginForm.DialogResult);
				if (loginForm.DialogResult != DialogResult.OK) return null;
				return new LotusNotesMeetingCaptureService.LNLoginData()
				{
					Password = loginForm.UserPassword,
					RememberMe = loginForm.RememberMe,
				};
			}
		}

		private class STAThreadRunner<T>
		{
			private readonly Func<T> funcToCallOnSTA;
			private readonly ManualResetEvent mreWait = new ManualResetEvent(false);
			private T result;

			private STAThreadRunner(Func<T> funcToCallOnSta)
			{
				this.funcToCallOnSTA = funcToCallOnSta;
			}

			private T GetResult()
			{
				var thread = new Thread(ThreadProc);
				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();
				mreWait.WaitOne();
				return result;
			}

			private void ThreadProc(object state)
			{
				Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);
				try
				{
					result = funcToCallOnSTA();
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unexpected error in func that have been called on STA thread.", ex);
				}
				finally
				{
					mreWait.Set();
				}
			}

			public static T CallOnSTA(Func<T> funcToCallOnSTA)
			{
				var data = new STAThreadRunner<T>(funcToCallOnSTA);
				return data.GetResult();
			}
		}
	}
}
