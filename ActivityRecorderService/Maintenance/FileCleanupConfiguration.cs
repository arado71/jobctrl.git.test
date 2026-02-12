using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Maintenance
{
	public struct FileCleanupConfiguration : IEquatable<FileCleanupConfiguration>
	{
		public int? CompanyId { get; private set; }
		public Storage Storage { get; private set; }

		public FileCleanupConfiguration(int? companyId, Storage storage) : this()
		{
			CompanyId = companyId;
			Storage = storage;
		}

		public FileCleanupConfiguration(LimitElement element) : this()
		{
			CompanyId = element.CompanyId;
			Storage = element.Storage;
		}

		public bool Equals(FileCleanupConfiguration other)
		{
			return CompanyId == other.CompanyId && Storage == other.Storage;
		}
	}
}
