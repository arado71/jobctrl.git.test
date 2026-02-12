using log4net;
using System;
using System.Collections.Generic;

namespace Tct.ActivityRecorderClient.Communication
{
	class GeneralLongIntervalQueryManager : PeriodicManager
	{
		private const int managerCallbackInterval = 3600 * 1000;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static GeneralLongIntervalQueryManager instance;
		private readonly List<ILongIntervalQuery> queries;

		protected override int ManagerCallbackInterval => managerCallbackInterval;

		public static GeneralLongIntervalQueryManager Instance => instance ?? (instance = new GeneralLongIntervalQueryManager());

		private GeneralLongIntervalQueryManager()
		: base(log, false)
		{
			queries = new List<ILongIntervalQuery>()
			{
				new GoogleAuthLongIntervalQuery(),
			};
		}

		protected override void ManagerCallbackImpl()
		{
			foreach (var longIntervalQuery in queries)
			{
				try
				{
					longIntervalQuery.DoWork();
				}
				catch (Exception ex)
				{
					log.Warn($"Uncaught exception in in long interval query: {longIntervalQuery.GetType().FullName}", ex);
				}
			}
		}
	}
}
