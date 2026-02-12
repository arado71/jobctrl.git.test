using System;
using System.Windows.Automation;
using System.Windows.Forms;
using JCAutomation.Data;
using log4net;

namespace JCAutomation.Managers
{
	public class CaptureManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ControlInfoFactory controlInfoFactory = new ControlInfoFactory();

		public event EventHandler<CaptureEventArgs> Capture;

		private bool focusMode = true;

		public bool FocusMode
		{
			get
			{
				return focusMode;
			}

			set
			{
				focusMode = value;
			}
		}

		public CaptureManager() : base(log)
		{
			
		}

		protected void OnCapture(ControlInfo activeControl)
		{
			var evt = Capture;
			if (evt != null) evt(this, new CaptureEventArgs(activeControl));
		}

		protected override void ManagerCallbackImpl()
		{
			AutomationElement element;
			if (focusMode)
			{
				element = AutomationElement.FocusedElement;
			}
			else
			{
				var pos = Cursor.Position;
				element = AutomationElement.FromPoint(new System.Windows.Point(pos.X, pos.Y));
			}

			if (element == null)
			{
				log.Debug("Failed to get any element");
				return;
			}

			var activeCtrlData = controlInfoFactory.Get(element, DetailLevel.WithParents, DetailLevel.Only);
			OnCapture(activeCtrlData);
		}

		protected override int ManagerCallbackInterval
		{
			get { return Configuration.CaptureInterval; }
		}
	}
}
