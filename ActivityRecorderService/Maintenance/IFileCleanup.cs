using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Maintenance
{
	public interface IFileCleanup
	{
		Storage Type { get; }
		ILookup<int,int> GetUserIds();
		IEnumerable<string> GetPaths(int companyId, int? userId);
	}
}
