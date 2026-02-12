using System;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	public interface INavigator
	{
		event EventHandler<SingleValueEventArgs<NavigationBase>> OnNavigate;

		void Up();
		void Goto(LocationKey navigation, bool leaveTrail = true);
	}
}