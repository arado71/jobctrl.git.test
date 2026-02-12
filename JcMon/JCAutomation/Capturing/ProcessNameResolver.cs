using Tct.ActivityRecorderClient;

namespace JCAutomation.Capturing
{
    using System;
    using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;

    public class ProcessNameResolver
    {
        private readonly Func<int, string> getProcessName = CachedFunc.Create<int, string>(new Func<int, string>(ProcessNameResolver.GetProcessNameImpl), TimeSpan.FromMinutes(2.0));
        public static readonly ProcessNameResolver Instance = new ProcessNameResolver();

        private ProcessNameResolver()
        {
        }

        public string GetProcessName(int processId)
	    {
		    return this.getProcessName(processId);
	    }

	    private static string GetProcessNameImpl(int processId)
        {
            string str;
            if (ProcessNameHelper.TryGetProcessName(processId, out str))
            {
                return str;
            }
            return "N/A";
        }
    }
}

