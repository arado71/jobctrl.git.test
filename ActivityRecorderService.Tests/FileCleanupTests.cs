using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Maintenance;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class FileCleanupTests
	{
		#region Cleanup by date

		[Fact]
		public void NullConfigurationCheck()
		{
			// Arrange
			using (var storage = new DummyStorage(Storage.Screenshot))
			{
				var files = new[]
				{
					new DummyStorage.DummyFile(1, DateTime.Now, 1024),
					new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1024),
					new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-10), 1024),
					new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-100), 1024),
				};
				var createdFiles = storage.AddFiles(files);
				var cleanup = new FileCleanup(storage);

				// Act
				cleanup.Cleanup(null);

				// Assert
				Assert.True(createdFiles.All(File.Exists));
			}
		}

		[Fact]
		public void DirectoryIsDeleted()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1d" }
				}
			};
			using (var storage = new DummyStorage(Storage.Screenshot))
			{
				var files = new[]
				{
					new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-10), 1024),
				};
				var createdFiles = storage.AddFiles(files);
				var cleanup = new FileCleanup(storage);

				// Act
				cleanup.Cleanup(config);

				// Assert
				Assert.True(createdFiles.All(n => !File.Exists(n)));
				Assert.True(storage.GetPaths(1, null).All(n => !Directory.Exists(n)));
			}
		}

		[Fact]
		public void DeleteDateYesterday()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1d" }
				}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteYesterdayStricterStoreOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1y" }, 
					new LimitElement() { MaxAge = "1d", Storage = Storage.Screenshot }
				}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), false, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), false, true),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), false, true),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, true),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateLastMonthInvalidLessStrictStoreOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1m" }, 
					new LimitElement() { MaxAge = "1y", Storage = Storage.Screenshot }
				}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateYesterdayTripleOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxAge = "1d" }, 
				new LimitElement() { MaxAge = "1y", CompanyId = 1 },
				new LimitElement() { MaxAge = "1m", CompanyId = 1, Storage = Storage.Screenshot}
			}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), false, true),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateLastMonthLessStrictCompanyOverride()
		{

			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1y" }, 
					new LimitElement() { MaxAge = "1m", CompanyId = 2 }
				}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1),true, true),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateLastYearMoreStrictCompanyOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1y" }, 
					new LimitElement() { MaxAge = "1m", CompanyId = 2 }
				}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateYesterdayStorage()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1d", Storage = Storage.Log }
				}
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, false),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), true, false),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), true, false),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), true, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), true, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), true, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateLastMonthGenericRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection { new LimitElement() { MaxAge = "1m" } }
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), false, false),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteDateLastYearGenericRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection { new LimitElement() { MaxAge = "1y" } }
			};

			var expectations = new[]
			{
				new FileDateExpectation(1, DateTime.Now, true, true),
				new FileDateExpectation(2, DateTime.Now, true, true),
				new FileDateExpectation(1, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddDays(-1), true, true),
				new FileDateExpectation(1, DateTime.Now.AddMonths(-1), true, true),
				new FileDateExpectation(2, DateTime.Now.AddMonths(-1), true, true),
				new FileDateExpectation(1, DateTime.Now.AddYears(-1), false, false),
				new FileDateExpectation(2, DateTime.Now.AddYears(-1), false, false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckDateCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}
		#endregion

		#region Cleanup by size

		[Fact]
		public void DeleteSizeAll()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = "0Kb" } 
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeAllButTwo()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b" } 
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeAllButFour()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 4+"b" } 
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeLessStrictCompanyRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b" },
				new LimitElement() { MaxSize = GetBlockSize() * 4+"b", CompanyId = 2}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeStricterCompanyRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 4+"b" },
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b", CompanyId = 2}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeInvalidLessStrictStorageRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b" },
				new LimitElement() { MaxSize = GetBlockSize() * 6+"b", Storage = Storage.Screenshot}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeLessStrictStorageRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 4+"b" },
				new LimitElement() { MaxSize = GetBlockSize() * 3+"b", Storage = Storage.Screenshot}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeStricterStorageRule()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 4+"b" },
				new LimitElement() { MaxSize = GetBlockSize() * 1+"b", Storage = Storage.Screenshot}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, false)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeLessStrictCompanyOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 1+"b", Storage = Storage.Screenshot},
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b", Storage = Storage.Screenshot, CompanyId = 1}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, true)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		[Fact]
		public void DeleteSizeStricterCompanyOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b", Storage = Storage.Screenshot},
				new LimitElement() { MaxSize = GetBlockSize() * 1+"b", Storage = Storage.Screenshot, CompanyId = 1}
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), true, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false, true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false, true)
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			using (var logStorage = new DummyStorage(Storage.Log))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage, logStorage);
			}
		}

		#endregion

		#region Date and size

		[Fact]
		public void DeleteDateAndSizeByDate()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 3+"b", MaxAge = "1m"},
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage);
			}
		}

		[Fact]
		public void DeleteDateAndSizeBySize()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b", MaxAge = "1y"},
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage);
			}
		}

		[Fact]
		public void DeleteDateAndSizeCompanyOverride()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
			{
				new LimitElement() { MaxSize = GetBlockSize() * 2+"b", MaxAge = "1y"},
				new LimitElement() { MaxSize = GetBlockSize() * 3+"b", MaxAge = "1m", CompanyId = 2},
			}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddMonths(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddMonths(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, DateTime.Now.AddYears(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, DateTime.Now.AddYears(-1), 1), false),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage);
			}
		}

		#endregion

		#region User level cleanup

		[Fact]
		public void DeleteUserLevelFiles()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1y", UserId = 1},
					new LimitElement() { MaxAge = "1m", UserId = 2},
				}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2,  DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now.AddMonths(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2, DateTime.Now.AddMonths(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now.AddMonths(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now.AddYears(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2, DateTime.Now.AddYears(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now.AddYears(-1), 1), true),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage);
			}
		}

		[Fact]
		public void DeleteUserLevelMixedGlobal()
		{
			// Arrange
			var config = new FileCleanupSection
			{
				Limits = new LimitElementCollection
				{
					new LimitElement() { MaxAge = "1m", UserId = 1},
					new LimitElement() { MaxAge = "6m", UserId = 2},
					new LimitElement() { MaxAge = "1y", CompanyId = 1},
				}
			};

			var expectations = new[]
			{
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2,  DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, 4, DateTime.Now, 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, 4, DateTime.Now.AddDays(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now.AddMonths(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2, DateTime.Now.AddMonths(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now.AddMonths(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, 4, DateTime.Now.AddMonths(-1), 1), true),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 1, DateTime.Now.AddYears(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 2, DateTime.Now.AddYears(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(1, 3, DateTime.Now.AddYears(-1), 1), false),
				new FileSizeExpectation(new DummyStorage.DummyFile(2, 4, DateTime.Now.AddYears(-1), 1), true),
			};

			// Act, Assert
			using (var screenshotStorage = new DummyStorage(Storage.Screenshot))
			{
				CheckSizeCleanup(expectations, config, screenshotStorage);
			}
		}


		#endregion

		private static void CheckDateCleanup(IEnumerable<FileDateExpectation> arranges, FileCleanupSection config, params DummyStorage[] storages)
		{
			var filesToKeep = new List<string>();
			var filesToDelete = new List<string>();
			for (int i = 0; i < storages.Length; i++)
			{
				foreach (var arrange in arranges)
				{
					var file = new DummyStorage.DummyFile(arrange.CompanyId, arrange.CreationDate, 1024);
					var filePath = storages[i].AddFile(file);
					if (arrange.Expected[i])
					{
						filesToKeep.Add(filePath);
					}
					else
					{
						filesToDelete.Add(filePath);
					}
				}
			}

			var cleanup = new FileCleanup(storages);

			// Act
			cleanup.Cleanup(config);

			// Assert
			Assert.True(filesToKeep.All(File.Exists));
			Assert.False(filesToDelete.Any(File.Exists));
		}

		private static void CheckSizeCleanup(IEnumerable<FileSizeExpectation> arranges, FileCleanupSection config, params DummyStorage[] storages)
		{
			var blockSize = GetBlockSize();
			var filesToKeep = new List<string>();
			var filesToDelete = new List<string>();
			for (int i = 0; i < storages.Length; i++)
			{
				foreach (var arrange in arranges)
				{
					var file = new DummyStorage.DummyFile(arrange.File.CompanyId, arrange.File.UserId, arrange.File.CreationDate, arrange.File.Size * blockSize);
					var filePath = storages[i].AddFile(file);
					if (arrange.Expected[i])
					{
						filesToKeep.Add(filePath);
					}
					else
					{
						filesToDelete.Add(filePath);
					}
				}
			}

			var cleanup = new FileCleanup(storages);

			// Act
			cleanup.Cleanup(config);

			// Assert
			Assert.True(filesToKeep.All(File.Exists), string.Format("{0} shouldn't be deleted!", filesToKeep.FirstOrDefault(x => !File.Exists(x))));
			Assert.False(filesToDelete.Any(File.Exists), string.Format("{0} is not removed!", filesToDelete.FirstOrDefault(File.Exists)));
		}

		private static long GetBlockSize()
		{
			return FileCleanup.GetBlockSize(Path.GetPathRoot(Path.GetTempPath()).TrimEnd('\\'));
		}

		private class FileDateExpectation
		{
			public FileDateExpectation(int companyId, DateTime creationDate, params bool[] expected)
			{
				CompanyId = companyId;
				CreationDate = creationDate;
				Expected = expected;
			}

			public int CompanyId { get; private set; }
			public DateTime CreationDate { get; private set; }
			public bool[] Expected { get; private set; }
		}

		private class FileSizeExpectation
		{
			public FileSizeExpectation(DummyStorage.DummyFile file, params bool[] expected)
			{
				File = file;
				Expected = expected;
			}

			public DummyStorage.DummyFile File { get; private set; }
			public bool[] Expected { get; private set; }
		}

		private class DummyStorage : IFileCleanup, IDisposable
		{
			private readonly Storage type;
			private readonly string basePath;
			private readonly List<string> createdFiles = new List<string>();

			public DummyStorage(Storage type)
			{
				this.type = type;
				basePath = GetTemporaryDirectory();
			}

			public string BasePath
			{
				get
				{
					return basePath;
				}
			}

			public Storage Type
			{
				get
				{
					return type;
				}
			}

			public string[] AddFiles(IEnumerable<DummyFile> files)
			{
				return files.Select(AddFile).ToArray();
			}

			public string AddFile(DummyFile file)
			{
				var folder = Path.Combine(basePath, file.CompanyId.ToString(CultureInfo.InvariantCulture));
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				var userFolder = Path.Combine(folder, file.UserId.ToString(CultureInfo.InvariantCulture), file.CreationDate.ToString("yy-MM-dd_HH-mm"));
				if (!Directory.Exists(userFolder))
					Directory.CreateDirectory(userFolder);

				var fileName = Path.Combine(userFolder, Type + "_" + (createdFiles.Count + 1) + ".tmp");
				using (var fileStream = File.Create(fileName))
				{
					fileStream.SetLength(file.Size);
					fileStream.Close();
				}

				createdFiles.Add(fileName);
				File.SetCreationTime(fileName, file.CreationDate);
				return fileName;
			}

			public ILookup<int, int> GetUserIds()
			{
				var result = new List<Tuple<int, int>>();
				List<string> directories;
				try
				{
					directories = Directory.EnumerateDirectories(basePath).ToList();
				}
				catch (IOException ex)
				{
					return result.ToLookup(k => k.Item1, v => v.Item2);
				}
				foreach (var dir in directories)
				{
					if (!int.TryParse(Path.GetFileName(dir), out var companyId)) continue;
					var subDirs = Directory.EnumerateDirectories(dir).ToList();
					foreach (var subDir in subDirs)
					{
						if (!int.TryParse(Path.GetFileName(subDir), out var userId)) continue;
						result.Add(Tuple.Create(companyId, userId)); 
					}
				}

				return result.ToLookup(k => k.Item1, v => v.Item2);
			}

			public IEnumerable<string> GetPaths(int companyId, int? userId)
			{
				var path = Path.Combine(basePath, companyId.ToString(CultureInfo.InvariantCulture));
				return new[] { userId.HasValue ? Path.Combine(path, userId.Value.ToString(CultureInfo.InvariantCulture)) : path };
			}

			private static string GetTemporaryDirectory()
			{
				var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(dir);
				return dir;
			}

			public struct DummyFile
			{
				public DummyFile(int companyId, int userId, DateTime creationDate, long size)
					: this()
				{
					this.CompanyId = companyId;
					this.UserId = userId;
					this.CreationDate = creationDate;
					this.Size = size;
				}

				public DummyFile(int companyId, DateTime creationDate, long size)
					: this(companyId, 1, creationDate, size)
				{
				}

				public int CompanyId { get; private set; }
				public int UserId { get; private set; }
				public DateTime CreationDate { get; private set; }
				public long Size { get; private set; }
			}

			public void Dispose()
			{
				foreach (var file in createdFiles.Where(File.Exists))
				{
					File.Delete(file);
				}
				DeleteDir(basePath);
			}

			private void DeleteDir(string basePath)
			{
				foreach (var dir in Directory.EnumerateDirectories(basePath))
				{
					DeleteDir(dir);
				}
				Directory.Delete(basePath);
			}
		}
	}
}
