using Tct.ActivityRecorderService.Ocr;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class OcrLearningTest
	{
		private static readonly TestDb testDb = new TestDb();
		static OcrLearningTest()
		{
			testDb.InitializeDatabase();
			Tct.ActivityRecorderService.Properties.Settings.Default["recorderConnectionString"] = testDb.ConnectionString;
		}
		[Fact]
		public void LearningmanagerTest()
		{
			LearningManager lm = new LearningManager();
			lm.Start();
		}
	}
}
