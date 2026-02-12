using System;
using System.Diagnostics;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	// NOT thread safe! Will be only called from UI
	public class MenuQuery
	{
		private static MenuQuery instance;

		public Versionable<ClientMenuLookup> ClientMenuLookup { get; private set; }
		public Versionable<SimpleWorkTimeStats> SimpleWorkTimeStats { get; private set; }

		public static MenuQuery Instance
		{
			get
			{
				DebugEx.EnsureGuiThread();
				return instance ?? (instance = new MenuQuery());
			}
		}

		private MenuQuery()
		{
			ClientMenuLookup = new Versionable<ClientMenuLookup>();
			SimpleWorkTimeStats = new Versionable<SimpleWorkTimeStats>();
		}
	}
}