using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	public class ProjectCost
	{
		public int ProjectId { get; set; }
		public string ProjectName { get; set; }
		public List<UserWorkCost> UserWorkCosts { get; set; }
		public List<ProjectCost> Childrens { get; set; }

		public WorkTimeAndCost GetProjectWorkTimeAndCost()
		{
			var result = new WorkTimeAndCost() { Cost = 0m };
			var userWorkCosts = GetFlattenedWorkCosts();
			foreach (var userWorkCost in userWorkCosts)
			{
				result.Cost += userWorkCost.Cost;
				result.WorkTime += userWorkCost.WorkTime;
			}
			return result;
		}

		public List<UserWorkCost> GetFlattenedWorkCosts()
		{
			var result = new List<UserWorkCost>();
			var stack = new Stack<ProjectCost>(); //avoid recursion
			stack.Push(this);
			while (stack.Count > 0)
			{
				var proj = stack.Pop();
				foreach (var childProj in proj.Childrens)
				{
					stack.Push(childProj);
				}
				result.AddRange(proj.UserWorkCosts);
			}
			return result;
		}

		public List<ProjectCost> GetFlattenedChildrenProjects()
		{
			var result = new List<ProjectCost>();
			var stack = new Stack<ProjectCost>(); //avoid recursion
			stack.Push(this);
			while (stack.Count > 0)
			{
				var proj = stack.Pop();
				foreach (var childProj in proj.Childrens)
				{
					result.Add(childProj);
					stack.Push(childProj);
				}
			}
			return result;
		}
	}
}
