using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu
{
	public class MenuEventArgs : EventArgs
	{
		public ClientMenu Menu { get; private set; }
		public ClientMenu OldMenu { get; private set; }
		public ClientMenuLookup MenuLookup { get; private set; }

		public MenuEventArgs(ClientMenu clientMenu, ClientMenu oldMenu, ClientMenuLookup clientMenuLookup = null)
		{
			if (clientMenu == null) throw new ArgumentNullException("clientMenu");
			if (clientMenuLookup == null) clientMenuLookup = new ClientMenuLookup() { ClientMenu = clientMenu };
			Menu = clientMenu;
			OldMenu = oldMenu ?? new ClientMenu();
			MenuLookup = clientMenuLookup;
		}
	}
}
