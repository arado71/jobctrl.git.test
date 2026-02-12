using System;
using System.Linq;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public class NavigationSuggestionView : NavigationBase
	{
		public NavigationSuggestionView(INavigator navigator)
			: base(LocationKey.Suggestion, navigator)
		{
			Icon = Resources.suggested;
			Render = RenderHint.Long;
		}

		public override void Localize()
		{
			Name = Labels.NavigationSuggestion;
		}

		protected override LocationKey[] GetChildren()
		{
			return null;
		}

		//protected override NavigationBase[] FetchChildren()
		//{
		//	return IntelligentSuggestionQuery.Instance.GetSuggestions().Where(x => x.WorkData.IsVisibleInMenu)
		//		.Select(x =>
		//		{
		//			NavigationBase n = NavigationFactory.Create(navigator, x);
		//			if (n != null)
		//			{
		//				n.Parent = this;
		//				n.IsFavorite = true;
		//			}
		//			return n;
		//		})
		//		.Where(n => n != null).ToArray();
		//}
	}
}