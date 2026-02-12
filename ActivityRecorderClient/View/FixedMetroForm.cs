using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.View
{
	public class FixedMetroForm : MetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		protected readonly SynchronizationContext context;
		protected bool isBusy;

		private bool border;
		/// <summary>
		/// Whether or not this control has a border
		/// </summary>
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Border
		{
			get
			{
				return border;
			}
			set
			{
				border = value;
				this.Invalidate();
			}
		}

		private int borderWidth = 1;
		/// <summary>
		/// The width (in pixels) of the controls border
		/// </summary>
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int BorderWidth
		{
			get
			{
				return borderWidth;
			}
			set
			{
				if (value <= 1) value = 1;
				borderWidth = value;
				UpdateBorderPen();
			}
		}

		private Color borderColor = Color.Black;
		/// <summary>
		/// The color of the controls border
		/// </summary>
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color BorderColor
		{
			get
			{
				return borderColor;
			}
			set
			{
				borderColor = value;
				UpdateBorderPen();
			}
		}

		private Pen borderPen = new Pen(Color.Black, 1);

		public FixedMetroForm()
		{
			AllowTransparency = false; //with WS_EX_COMPOSITED this will reduce mini/maximize flickering http://stackoverflow.com/questions/2612487/how-to-fix-the-flickering-in-user-controls but metro textbox would flicker (AllowTransparency alone is better than nothing)
			ShadowType = MetroFormShadowType.SystemShadow;
			Border = true;
			BorderColor = Color.Black;
			context = SynchronizationContext.Current;
		}

		private void UpdateBorderPen()
		{
			if (borderPen.Color == BorderColor && (int)borderPen.Width == BorderWidth) return;
			borderPen.Dispose();
			borderPen = new Pen(BorderColor, BorderWidth);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (isBusy)
			{
				var res = MessageBox.Show(this, Labels.Form_ConfirmOperationCancelBody, Labels.Form_ConfirmOperationCancelTitle, MessageBoxButtons.OKCancel);
				log.InfoFormat("Operation abort confirmation result is {0}", res);
				e.Cancel = res != DialogResult.OK;
			}

			base.OnClosing(e);
		}

		protected virtual void SetBusyImpl(bool isBusy)
		{
		}

		protected void SetBusy(bool isBusy)
		{
			this.isBusy = isBusy;
			SetBusyImpl(isBusy);
		}

		protected void BackgroundQuery<T>(Func<GeneralResult<T>> queryFunc, Action<T> onSuccess, Action<Exception> onFaulted)
		{
			if (onSuccess == null) throw new ArgumentNullException("onSuccess");
			if (onFaulted == null) throw new ArgumentNullException("onFaulted");
			SetBusy(true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var funcResult = queryFunc();
				context.Post(__ =>
				{
					if (IsDisposed) return;
					SetBusy(false);
					if (funcResult.Exception != null)
					{
						onFaulted(funcResult.Exception);
					}
					else
					{
						onSuccess(funcResult.Result);
					}
				}, null);
			});
		}

		protected void BackgroundForcableQuery<T>(Func<bool, GeneralResult<T>> queryFunc, Action<T> onSuccess, string errorMessage)
		{
			BackgroundForcableQuery(queryFunc, onSuccess, ex => DefaultBackgroundQueryOnError(ex, errorMessage));
		}

		protected void BackgroundForcableQuery<T>(Func<bool, GeneralResult<T>> queryFunc, Action<T> onSuccess, Action<Exception> onFaulted)
		{
			BackgroundQuery(() => queryFunc(false), onSuccess, ex =>
			{
				var validationException = ex as ValidationException;
				if (validationException != null)
				{
					var sb = new StringBuilder();
					switch (validationException.Severity)
					{
						case Severity.Error:
							
							sb.AppendLine(Labels.Worktime_ManualModificationError);
							foreach (var reason in validationException.Result.Where(x => x.Severity == Severity.Error))
							{
								sb.Append("• ");
								sb.AppendLine(reason.Message);
							}

							MessageBox.Show(this, sb.ToString(), Labels.Warning, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
							break;
						case Severity.Warn:
							sb.AppendLine(Labels.Worktime_ValidationExplain);
							foreach (var reason in validationException.Result.Where(x => x.Severity == Severity.Warn))
							{
								sb.Append("• ");
								sb.AppendLine(reason.Message);
							}

							sb.AppendLine();
							sb.Append(Labels.Worktime_ValidationSure);

							if (MessageBox.Show(this, sb.ToString(), Labels.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) ==
							    DialogResult.Yes)
							{
								log.Info("User replied yes for forcing");
								BackgroundQuery(() => queryFunc(true), onSuccess, onFaulted);
							}
							else
							{
								log.Info("User replied no for forcing");
							}
							break;
						default:
							return;
					}
				}
				else
				{
					onFaulted(ex);
				}
			});
		}

		protected void DefaultBackgroundQueryOnError(Exception ex, string errorMessage)
		{
			var validationException = ex as ValidationException;
			if (validationException != null)
			{
				log.Info("Validation failed", validationException);
				MessageBox.Show(this, validationException.Message, Labels.Error, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}

			log.Warn("Error while processing background query", ex);
			MessageBox.Show(this, errorMessage, Labels.Error, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}

		protected void BackgroundQuery<T>(Func<GeneralResult<T>> queryFunc, Action<T> onSuccess, string errorMessage)
		{
			BackgroundQuery(queryFunc, onSuccess, ex => DefaultBackgroundQueryOnError(ex, errorMessage));
		}

		private const int metroTopBorder = 5;
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (!Border) return;

			var topLeft = new Point(ClientRectangle.X, ClientRectangle.Y + metroTopBorder);
			var bottomLeft = new Point(ClientRectangle.X, ClientRectangle.Y + ClientRectangle.Height - 1);
			var bootomRight = new Point(ClientRectangle.X + ClientRectangle.Width - 1, ClientRectangle.Y + ClientRectangle.Height - 1);
			var topRight = new Point(ClientRectangle.X + ClientRectangle.Width - 1, ClientRectangle.Y + metroTopBorder);

			e.Graphics.DrawLines(borderPen, new[] { topLeft, bottomLeft, bootomRight, topRight });
		}

		public bool IsVisibleOnScreen()
		{
			foreach (var screen in Screen.AllScreens)
			{
				if (screen.WorkingArea.Contains(this.DesktopLocation))
				{
					return true;
				}
			}

			return false;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WinApi.WM_SETTINGCHANGE && m.WParam.ToInt32() == WinApi.SPI_WORKAREA)
			{
				MoveIfApplicable();
			}

			base.WndProc(ref m);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			MoveIfApplicable();
			base.OnVisibleChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				borderPen.Dispose();
			}

			base.Dispose(disposing);
		}

		private void MoveIfApplicable()
		{
			if (!Visible || IsVisibleOnScreen()) return;
			DesktopLocation = new Point(100, 100);
		}
	}
}
