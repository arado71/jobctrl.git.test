using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model.Jira
{
	public static class PriorityConverter
	{
		public static int ConvertJiraPriorityToJcPriority(string priority)
		{
			int res = 999;
			switch (priority.ToLower())
			{
				case "highest":
					res = 900;
					break;
				case "high":
					res = 700;
					break;
				case "medium":
					res = 500;
					break;
				case "low":
					res = 300;
					break;
				case "lowest":
					res = 100;
					break;
				default:
					break;
			}
			return res;
		}
	}
}
