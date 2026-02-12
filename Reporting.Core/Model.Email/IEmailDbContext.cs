using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Model.Email
{
	public interface IEmailDbContext
	{
		List<ICollectedItem> GetCollectedItems(int[] userIds, DateTime startDate, DateTime endDate);

		List<IWorkItem> GetWorkItems(int[] userIds, DateTime startDate, DateTime endDate);

		List<IWorkItemDeletion> GetDeletions(int[] userIds, DateTime startDate, DateTime endDate);
		
		List<IEmailUser> GetUsers(int[] userIds);
	}
}
