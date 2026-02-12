using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Storage
{
	public interface IScreenShotPathResolver
	{
		string Data { get; }
		ILookup<int, int> GetUserIds();
		IEnumerable<string> GetPaths(int companyId, int? userId);
		void GetPath(ScreenShot screenShot, bool forWrite, out string dir, out string fileName, out long offset, out int length);
	}
}
