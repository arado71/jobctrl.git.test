using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Coordinates messages between DesktopCaptureManager and CaptureManager on the GUI thread.
	/// Used by CurrentWorkController and CaptureCoordinator. Basically sends DesktopLayouts to CaptureManager.
	/// </summary>
	public class WorkAndLayoutGuiCoordinator
	{
		private readonly CaptureManager captureManager;

		private DesktopCapture currentCapture;

		public WorkAndLayoutGuiCoordinator(CaptureManager captureManager)
		{
			this.captureManager = captureManager;
		}

		//called on the GUI Thread by CurrentWorkController
		public void StopWork()
		{
			captureManager.StopWork();
			currentCapture = null;
		}

		//called on the GUI Thread by CurrentWorkController
		public void StartWork(WorkData workData)
		{
			captureManager.StartWork(workData.Id.Value, currentCapture, workData.AssignData);
			currentCapture = null;
		}

		//called on the GUI Thread by CaptureCoordinator
		public IDisposable SetDesktopCapture(DesktopCapture desktopCapture)
		{
			if (desktopCapture == null) return null;
			currentCapture = desktopCapture;
			return new DesktopCaptureWriter(this);
		}

		private void SetDesktopCaptureImpl()
		{
			if (currentCapture == null) return;
			captureManager.SetDesktopCapture(currentCapture);
			currentCapture = null;
		}

		private class DesktopCaptureWriter : IDisposable
		{
			private readonly WorkAndLayoutGuiCoordinator parent;

			public DesktopCaptureWriter(WorkAndLayoutGuiCoordinator parent)
			{
				this.parent = parent;
#if !DEBUG
			}
#else
				Debug.Assert(!IsWriterActive);
				IsWriterActive = true; //there can be at most one writer active at a given time
			}

			private static bool IsWriterActive;
#endif
			public void Dispose()
			{
				parent.SetDesktopCaptureImpl();
#if !DEBUG
			}
#else
				Debug.Assert(IsWriterActive);
				IsWriterActive = false;
				GC.SuppressFinalize(this);
			}

			~DesktopCaptureWriter()
			{
				Debug.Fail("DesktopCpatureWriter is leaked");
			}
#endif
		}

	}
}
