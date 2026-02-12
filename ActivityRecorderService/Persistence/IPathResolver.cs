using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Persistence
{
	public interface IPathResolver
	{
		string GetFilePath(object obj);
		string GetRootDir();
		string GetSearchPattern();
	}

	public interface IPathResolver<in T> : IPathResolver
	{
		string GetFilePath(T obj);
	}
}
