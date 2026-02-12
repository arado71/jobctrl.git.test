using System;

namespace Tct.ActivityRecorderClient.Common
{
    [Flags]
    public enum DiagnosticOperationMode
    {
        Enabled                        = 1 << 0,
        DisableDomCapture              = 1 << 1 | Enabled,
        DisableOutlookJcMailCapture    = 1 << 2 | Enabled,
        DisableOutlookAddinCapture     = 1 << 3 | Enabled,
        DisableAutomationCapture       = 1 << 4 | Enabled,
        DisableAllPluginCapture        = 1 << 5 | Enabled,
        DisableUrlCapture              = 1 << 6 | Enabled,
        DisableTitleCapture            = 1 << 7 | Enabled,
        DisableProcessCapture          = 1 << 8 | Enabled,
        None                           = 0,
    }
}