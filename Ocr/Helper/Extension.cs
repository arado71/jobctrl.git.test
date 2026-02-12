using System;
using log4net;

namespace Ocr.Helper
{
	public static class Extension
	{
		private static readonly Type callerStackBoundaryType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
		public static void VerboseFormat(this ILog log, string format, params object[] args)
		{
			if (log.Logger.IsEnabledFor(log4net.Core.Level.Verbose))
			{
				log.Logger.Log(callerStackBoundaryType, log4net.Core.Level.Verbose,
					new log4net.Util.SystemStringFormat(System.Globalization.CultureInfo.InvariantCulture, format, args), null);
			}
		}
	}
}
