using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.Caching.Works
{
	public class WorkHierarchyService
	{
		internal static Func<IWorkHierarchyService> FactoryForInstance = null; //override Service factory for unit tests
		public static IWorkHierarchyService Instance { get { return Nested.LazyIstance; } }

		private WorkHierarchyService()
		{
		}

		private static IWorkHierarchyService CreateDefaultInstance()
		{
			var instance = new WorkHierarchyLite();
			instance.Start();
			return instance;
		}

		private class Nested
		{
			internal static readonly IWorkHierarchyService LazyIstance;

			static Nested()
			{
				var fact = FactoryForInstance ?? CreateDefaultInstance;
				LazyIstance = fact();
			}
		}
	}
}
