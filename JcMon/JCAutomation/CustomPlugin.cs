namespace JCAutomation
{
    using System;
    using System.Windows.Automation;

    public class CustomPlugin
    {
        private readonly Func<IntPtr, int, string, AutomationElement> capture;

        public CustomPlugin(Func<IntPtr, int, string, AutomationElement> captureFunc)
        {
            this.capture = captureFunc;
        }

	    public AutomationElement Capture(IntPtr hWnd, int processId, string processName)
	    {
			return this.capture(hWnd, processId, processName);
	    }
    }
}

