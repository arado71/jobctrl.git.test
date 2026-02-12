using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookInteropService
{
	//http://msdn.microsoft.com/en-us/library/office/ff865653.aspx
	public enum OlMailRecipientType
	{
		olBCC = 3, //The recipient is specified in the BCC property of the Item.
		olCC = 2, //The recipient is specified in the CC property of the Item.
		olOriginator = 0, //Originator (sender) of the Item.
		olTo = 1, //The recipient is specified in the To property of the Item.
	}
}
