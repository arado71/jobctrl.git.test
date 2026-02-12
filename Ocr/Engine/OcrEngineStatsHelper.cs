namespace Ocr.Engine
{
	public static class OcrEngineStatsHelper
	{
		private static long Timeouts;
		private static long Total;
		private static double Max;
		private static double Sum;

		public static void Add(double data)
		{
			Total++;
			if (data > Max)
			{
				Max = data;
			}
			Sum += data;
		}

		public static void AddTimeout()
		{
			Timeouts++;
		}

		public static void ClearStats()
		{
			Timeouts = 0;
			Total = 0;
			Max = 0;
		}

		public static long GetTimeouts()
		{
			return Timeouts;
		}

		public static long GetTotal()
		{
			return Total;
		}

		public static double GetMax()
		{
			return Max;
		}

		public static double GetAverage()
		{
			return Sum / Total;
		}
	}
}
