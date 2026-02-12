using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Persistence
{
	/// <summary>
	/// Thread-safe class for resolving paths for DeadLetterItems
	/// </summary>
	public class DeadLetterPathResolver : IPathResolver<DeadLetterItem>
	{
		private readonly string rootDir;

		public DeadLetterPathResolver(string rootDir)
		{
			this.rootDir = rootDir;
		}

		public string GetFilePath(DeadLetterItem item)
		{
			return item.UserId + "_" + item.ItemType + "_" + item.Id + "_" + item.StartDate.ToString("yyyy-MM-dd") + ".dead";
		}

		string IPathResolver.GetFilePath(object obj)
		{
			return GetFilePath(obj as DeadLetterItem);
		}

		public string GetRootDir()
		{
			return rootDir;
		}

		public string GetSearchPattern()
		{
			return "*.dead";
		}
	}
}
