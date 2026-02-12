using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	// Assumes ClientMenuLookup is immutable (or at least won't change during execution)
	public class NavigationFactory
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<LocationKey, NavigationBase> elements = new Dictionary<LocationKey, NavigationBase>();
		private readonly Dictionary<LocationKey, int> referenceCounter = new Dictionary<LocationKey, int>();
		private readonly INavigator navigator;

		public NavigationFactory(INavigator navigator)
		{
			this.navigator = navigator;
		}

		public NavigationBase Get(LocationKey reference)
		{
			if (!elements.ContainsKey(reference))
			{
				if (!Register(reference)) return null;
			}

			referenceCounter[reference]++;
			return elements[reference];
		}

		public void Release(NavigationBase navigation)
		{
			if (navigation == null) return;
			var key = navigation.Key;
			if (!referenceCounter.ContainsKey(key))
			{
				log.ErrorAndFail(string.Format("Invalid release of navigation {0}", navigation.Id));
				return;
			}

			if (--referenceCounter[key] == 0)
			{
				Debug.Assert(elements.ContainsKey(key));
				referenceCounter.Remove(key);
				elements[key].Dispose();
				elements.Remove(key);
			}
		}

		private bool Register(LocationKey reference)
		{
			Debug.Assert(!referenceCounter.ContainsKey(reference));
			Debug.Assert(!elements.ContainsKey(reference));
			switch (reference.Type)
			{
				case LocationKey.LocationKeyType.Work:
					var work = MenuQuery.Instance.ClientMenuLookup.Value.GetWorkDataWithParentNames(reference.Id);
					if (work == null)
					{
						log.ErrorAndFail("Cannot find work id " + reference.Id);
						return false;
					}
					Register(new NavigationWork(navigator, work));
					return true;
				case LocationKey.LocationKeyType.ClosedWork:
					var recents = RecentClosedHelper.GetRecents();
					var closedWork =
						recents.FirstOrDefault(x => x.WorkData != null && x.WorkData.Id != null && x.WorkData.Id.Value == reference.Id);
					if (closedWork == null)
					{
						log.ErrorAndFail("Invalid raw WorkData");
						return false;
					}
					Register(new NavigationClosedWork(navigator, closedWork));
					return true;
				case LocationKey.LocationKeyType.Project:
					WorkDataWithParentNames project;
					if (!MenuQuery.Instance.ClientMenuLookup.Value.ProjectDataById.TryGetValue(reference.Id, out project))
					{
						log.ErrorAndFail("Cannot find project id " + reference.Id);
						return false;
					}
					Register(new NavigationProject(navigator, project));
					return true;
				case LocationKey.LocationKeyType.MenuItem:
					if (LocationKey.Root.Equals(reference))
					{
						Register(new NavigationRootView(navigator, this));
						return true;
					}
					if (LocationKey.Deadline.Equals(reference))
					{
						Register(new NavigationDeadlineView(navigator));
						return true;
					}
					if (LocationKey.Favorite.Equals(reference))
					{
						Register(new NavigationFavoriteView(navigator));
						return true;
					}
					if (LocationKey.Priority.Equals(reference))
					{
						Register(new NavigationPriorityView(navigator));
						return true;
					}
					if (LocationKey.Progress.Equals(reference))
					{
						Register(new NavigationProgressView(navigator));
						return true;
					}
					if (LocationKey.Recent.Equals(reference))
					{
						Register(new NavigationRecentView(navigator));
						return true;
					}
					if (LocationKey.RecentProject.Equals(reference))
					{
						Register(new NavigationRecentProjectView(navigator));
						return true;
					}
					if (LocationKey.Suggestion.Equals(reference))
					{
						Register(new NavigationSuggestionView(navigator));
						return true;
					}
					if (LocationKey.All.Equals(reference))
					{
						Register(new NavigationAllView(navigator));
						return true;
					}
					if (LocationKey.RecentClosed.Equals(reference))
					{
						Register(new NavigationRecentClosedView(navigator));
						return true;
					}
					log.ErrorAndFail("Invalid MenuItem id " + reference.Id);
					return false;
				default:
					log.ErrorAndFail("Invalid reference type " + reference.Type);
					return false;
			}
		}

		private void Register(NavigationBase navigation)
		{
			var key = navigation.Key;
			referenceCounter.Add(key, 0);
			elements.Add(key, navigation);
		}
	}
}