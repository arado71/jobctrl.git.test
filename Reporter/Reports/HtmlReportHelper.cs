using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reporter.Email;
using Reporter.Mobile;
using Reporter.Reports;

namespace Reporter.Reports
{
	public static class HtmlReportHelper
	{
		//var users = DbHelper.GetUserIdsForCompany(2);
		//users.ToArray(), DateTime.UtcNow.AddDays(-40), DateTime.UtcNow
		public static void GeneratePhoneCallReport(string outputDir, int companyId, DateTime startDate, DateTime endDate)
		{
			var users = new MobileDbContext().GetUserIdsForCompany(2).ToArray();
			GeneratePhoneCallReport(outputDir, users, startDate, endDate);
		}

		public static void GenerateEmailReport(string outputDir, int[] userIds, DateTime startDate, DateTime endDate)
		{
			var report = new EmailCommunicationReport(new EmailDbContext());

			var result = report.GenerateReport(userIds, startDate, endDate);

			string users, matrix;
			result.SerializeTo(out users, out matrix);

			Directory.CreateDirectory(outputDir);
			File.WriteAllText(Path.Combine(outputDir, "users.csv"), users);

			File.WriteAllText(Path.Combine(outputDir, "matrix.json"), matrix);

			File.WriteAllText(Path.Combine(outputDir, "mobile.html"), LoadResource("mobile.html"));
		}

		public static void GeneratePhoneCallReport(string outputDir, int[] userIds, DateTime startDate, DateTime endDate)
		{
			var report = new MobileCommunicationReport(new MobileDbContext());

			var result = report.GenerateReport(userIds, startDate, endDate);

			string users, matrix;
			result.SerializeTo(out users, out matrix);

			Directory.CreateDirectory(outputDir);
			File.WriteAllText(Path.Combine(outputDir, "users.csv"), users);

			File.WriteAllText(Path.Combine(outputDir, "matrix.json"), matrix);

			File.WriteAllText(Path.Combine(outputDir, "mobile.html"), LoadResource("mobile.html"));
		}

		private static string LoadResource(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof(HtmlReportHelper).Namespace + "." + fileName;
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					Debug.Fail(fileName + " resource not found");
					return null;
				}
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
