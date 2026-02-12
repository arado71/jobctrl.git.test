using System.Globalization;

namespace MailActivityTracker
{
    public static class LocalizationHelper
    {
        public static void InitLocalization()
        {
#if DEBUG
            Labels.Culture = new CultureInfo("hu-HU");
#else
            Labels.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
#endif
        }
    }
}
