namespace JCAutomation
{
    using System;
    using System.Windows.Automation;

    public class RunningPluginInfo : AutomationElementInfo
    {
        private string scanTime;
        private IntPtr topLevelHandle;

        public RunningPluginInfo(AutomationElement element) : base(element)
        {
        }

        public string ScanTime
        {
	        get
	        {
		        return
			        this.scanTime;
	        }
	        set
            {
                base.UpdateField<string>(ref this.scanTime, value, "ScanTime");
            }
        }

        public IntPtr TopLevelHandle
        {
	        get
	        {
		        return
			        this.topLevelHandle;
	        }
	        set
            {
                base.UpdateField<IntPtr>(ref this.topLevelHandle, value, "TopLevelHandle");
            }
        }
    }
}

