using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference
{
	public partial class MailCapture
	{
		public string GetToString()
		{
			var sb = new StringBuilder();
			if (To != null && To.Count > 0)
			{
				sb.Append(To[0]);
				for (int i = 1; i < To.Count; i++)
				{
					sb.Append(";").Append(To[i]);
				}
			}
			return sb.ToString();
		}

		public string GetCcString()
		{
			var sb = new StringBuilder();
			if (Cc != null && Cc.Count > 0)
			{
				sb.Append(Cc[0]);
				for (int i = 1; i < Cc.Count; i++)
				{
					sb.Append(";").Append(Cc[i]);
				}
			}
			return sb.ToString();
		}

		public string GetToEmailString()
		{
			var sb = new StringBuilder();
			if (To != null && To.Count > 0)
			{
				sb.Append(To[0].Email);
				for (int i = 1; i < To.Count; i++)
				{
					sb.Append(";").Append(To[i].Email);
				}
			}
			return sb.ToString();
		}

		public string GetCcEmailString()
		{
			var sb = new StringBuilder();
			if (Cc != null && Cc.Count > 0)
			{
				sb.Append(Cc[0].Email);
				for (int i = 1; i < Cc.Count; i++)
				{
					sb.Append(";").Append(Cc[i].Email);
				}
			}
			return sb.ToString();
		}
	}
}
