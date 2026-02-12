using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Caching.Works
{
	public abstract class WorkHierarchyBase
	{
		private const string defaultSeparator = " \u00BB ";
		private const string defaultEllipse = "...";
		private const int defaultWorkNameMinLen = 12;
		private const int defaultProjNameMinLen = 6;

		protected internal abstract bool TryGetWork(int workId, out Work work);
		protected internal abstract bool TryGetProject(int projectId, out Project project);

		public string GetWorkNameWithProjects(int workId, int targetLength = int.MaxValue)
		{
			return GetWorkNameImpl(workId, true, targetLength);
		}

		public string GetWorkName(int workId, int targetLength = int.MaxValue)
		{
			return GetWorkNameImpl(workId, false, targetLength);
		}

		private static readonly string[] noProj = new string[0];
		private string GetWorkNameImpl(int workId, bool includeProjects, int targetLength)
		{
			string workName;
			string[] projs;
			Work work;
			if (!TryGetWork(workId, out work))
			{
				workName = EmailStats.EmailStats.UnknownWork;
				projs = noProj;
			}
			else
			{
				workName = work.Name;
				projs = includeProjects
					? GetParentProjects(work)
						.Reverse()
						.Skip(1) //skip the root because it's the name of the company
						.Select(n => n.Name)
						.ToArray()
					: noProj;
			}
			return GetWorkAndProjectNamesWithEllipse(workName, projs, targetLength);
		}

		public IEnumerable<Project> GetParentProjects(Work work)
		{
			if (work == null) yield break;
			int? projectId = work.ProjectId;
			Project project;
			while (projectId.HasValue && TryGetProject(projectId.Value, out project))
			{
				yield return project;
				projectId = project.ParentId;
			}
		}

		internal static string GetWorkAndProjectNamesWithEllipse(string workName, string[] projectNames, int targetLength, string separator = defaultSeparator, string ellipse = defaultEllipse, int workNameMinLen = defaultWorkNameMinLen, int projNameMinLen = defaultProjNameMinLen)
		{
			//short path
			if (targetLength == int.MaxValue)
			{
				if (projectNames.Length == 0)
				{
					return workName;
				}
				else
				{
					var sbs = new StringBuilder();
					foreach (var projectName in projectNames)
					{
						sbs.Append(projectName).Append(separator);
					}
					sbs.Append(workName);
					return sbs.ToString();
				}
			}
			//long path
			var deletableChars = new int[projectNames.Length + 1];
			var sum = workName.Length;
			deletableChars[projectNames.Length] = Math.Max(0, workName.Length - workNameMinLen - ellipse.Length);
			var delSum = deletableChars[projectNames.Length]; //sum of deletableChars
			for (int i = 0; i < projectNames.Length; i++)
			{
				deletableChars[i] = Math.Max(0, projectNames[i].Length - projNameMinLen - ellipse.Length);
				sum += projectNames[i].Length + separator.Length;
				delSum += deletableChars[i];
			}
			var toDelete = sum - targetLength;
			if (toDelete <= 0)
			{
				delSum = 0;
				Array.Clear(deletableChars, 0, deletableChars.Length); //we don't have to delete anything
			}
			else
			{
				var found = true;
				while (found && delSum > toDelete)
				{
					found = false;
					for (int i = deletableChars.Length - 1; i >= 0; i--) //we have more chars at the end
					{
						if (deletableChars[i] > 0)
						{
							found = true;
							deletableChars[i]--;
							delSum--;
						}
						if (delSum == toDelete) break;
					}
				}
			}
			var sb = new StringBuilder(sum - delSum);
			for (int i = 0; i < projectNames.Length; i++)
			{
				InsertEllipse(sb, projectNames[i], deletableChars[i], ellipse);
				sb.Append(separator);
			}
			InsertEllipse(sb, workName, deletableChars[deletableChars.Length - 1], ellipse);
			return sb.ToString();
		}

		private static void InsertEllipse(StringBuilder sb, string text, int deleteNum, string ellipse)
		{
			if (deleteNum == 0)
			{
				sb.Append(text);
			}
			else
			{
				var newLen = text.Length - deleteNum - ellipse.Length; //new length without the ellipse
				sb.Append(text, 0, newLen - newLen / 2); //first part can be longer
				sb.Append(ellipse);
				sb.Append(text, text.Length - newLen / 2, newLen / 2);
			}
		}
	}
}
