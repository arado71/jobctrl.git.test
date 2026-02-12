using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Tct.ActivityRecorderClient.Serialization;
using log4net;

namespace Tct.ActivityRecorderClient
{
	public static class LocalizationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static string LocalizationPath { get { return "Language-" + ConfigManager.UserId; } }

		public static IEnumerable<CultureInfo> GetSupportedCultures()
		{
			return new[]
			{
				new CultureInfo("en-US"), //the new default (used by Login form)
				new CultureInfo("hu-HU"),
				new CultureInfo("pt-BR"),
				new CultureInfo("ja-JP"),
				new CultureInfo("ko-KR"),
				new CultureInfo("es-MX"),
			};
		}

		public static void InitLocalization()
		{
			CultureInfo loadedCulture;
			if (Labels.Culture != null)
			{
				//do nothing culture is already set
			}
			else if (IsolatedStorageSerializationHelper.Exists(LocalizationPath)
				&& IsolatedStorageSerializationHelper.Load(LocalizationPath, out loadedCulture))
			{
				Labels.Culture = loadedCulture;
			}
			else
			{
				Labels.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			}
			log.Info("Using culture " + Labels.Culture.EnglishName);
			Debug.Assert(GetAvailableCultures().All(n => GetSupportedCultures().Contains(n)), "Some cultures are missing");
		}

		public static void SaveLocalization(CultureInfo culture)
		{
			if (culture == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(LocalizationPath))
				{
					IsolatedStorageSerializationHelper.Delete(LocalizationPath);
				}
			}
			else
			{
				IsolatedStorageSerializationHelper.Save(LocalizationPath, culture);
			}
		}

		//too slow
		private static List<CultureInfo> GetAvailableCultures()
		{
			var result = new List<CultureInfo>();
			var currAsm = Assembly.GetExecutingAssembly();
			foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
			{
				try
				{
					currAsm.GetSatelliteAssembly(ci);
					result.Add(ci);
				}
				catch
				{
				}
			}
			return result;
		}
	}
}
