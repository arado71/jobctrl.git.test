using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraSyncTool.Jira.Model.Jc
{
	public class WorkTime
	{
		public Task Task { get; set; }
		private long duration;
		public long Duration
		{
			get
			{
				return duration;
			}
			set
			{
				duration = value;
				setJiraDuration();
			}
		}

		public DateTime StartDate { get; set; }

		public int UserId { get; set; }

		public string UserEmail { get; set; }

		public string Description { get; set; }

		private string jiraDuration;
		public string JiraDuration
		{
			get
			{
				return jiraDuration;
			}
		}

		public long Remaining { get; private set; }

		private void setJiraDuration()
		{
			StringBuilder res = new StringBuilder();
			long total = duration;
			if (total > 60)
			{
				var mod = total % 60;
				if (mod > 30)
				{
					total += 60;
					Remaining = mod - 60;
				}
				else
				{
					Remaining = mod;
				}
				total -= mod;
			}
			else
			{
				total = 60;
				Remaining = 60 - total;
			}
			
			if (total / 60 > 0)
			{
				total /= 60;
				if (total % 60 != 0)
					res.Insert(0, total % 60 + "m ");
				if (total / 60 > 0)
				{
					total /= 60;
					if(total % 8 != 0)
						res.Insert(0, total % 8 + "h ");
					if(total/8 >0)
					{
						total /= 8;
						if (total % 5 != 0)
							res.Insert(0, total % 5 + "d ");
						total /= 5;
						if (total > 0)
							res.Insert(0, total + "w ");
					}
				}
			}
			res.Remove(res.Length-1, 1);
			jiraDuration = res.ToString();
		}
	}
}
