using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderService;
using Xunit;
using Xunit.Sdk;

namespace Tct.Tests.ActivityRecorderService
{
	public class StreamDataUploadStoreTests : IDisposable
	{
		private readonly StreamDataUploadStore store = new StreamDataUploadStore();
		private static readonly string[] files = new[] { Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "test.tmp") };

		public StreamDataUploadStoreTests()
		{
			DeleteFiles();
		}

		[Fact]
		public void CanOpen()
		{
			var data = new StreamDataTest();
			store.Open(data);
		}

		[Fact]
		public void CanOpenTwice()
		{
			var data = new StreamDataTest();
			store.Open(data);
			store.Open(data);
		}

		[Fact]
		public void CanOpenBigOffset()
		{
			var data = new StreamDataTest() { Offset = 1 };
			store.Open(data);
		}

		[Fact]
		public void CanOpenBigOffsetAfterClosed()
		{
			var data = new StreamDataTest() { Offset = 1 };
			store.Open(data);
			store.Close(data);
			store.Open(data);
		}

		[Fact]
		public void CanOpenAndWrite()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(data);
			store.AddData(data);
			Assert.True(b.SequenceEqual(ReadAllBytes(data.GetPath())));
		}

		[Fact]
		public void CanOpenAndClose()
		{
			var data = new StreamDataTest();
			store.Open(data);
			store.Close(data);
		}


		[Fact]
		public void CanCloseTwice()
		{
			var data = new StreamDataTest();
			store.Close(data);
			store.Close(data);
		}

		[Fact]
		public void CanReopen()
		{
			var data = new StreamDataTest();
			store.Open(data);
			store.Close(data);
			store.Open(data);
		}

		[Fact]
		public void CanOpenAndWriteAndClose()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(new StreamDataTest());
			store.AddData(data);
			store.Close(new StreamDataTest());
			Assert.True(b.SequenceEqual(ReadAllBytes(data.GetPath())));
			Assert.True(File.Exists(new StreamDataTest().GetPath()));
		}

		[Fact]
		public void CanDelete()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(new StreamDataTest());
			store.AddData(data);
			store.Delete(new StreamDataTest());
			Assert.False(File.Exists(new StreamDataTest().GetPath()));
		}

		[Fact]
		public void CanOpenAndWriteAndCloseDataIgnoredAtOpenClose()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(data);
			store.AddData(data);
			store.Close(data);
			Assert.True(b.SequenceEqual(ReadAllBytes(data.GetPath())));
		}

		[Fact]
		public void CanOpenAndWriteTwiceAndClose()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(new StreamDataTest());
			store.AddData(data);
			store.AddData(new StreamDataTest() { Offset = b.Length, Data = b });
			store.Close(new StreamDataTest());
			Assert.True(b.Concat(b).SequenceEqual(ReadAllBytes(data.GetPath())));
		}

		[Fact]
		public void CanOpenAndWriteTwiceAndCloseDataIgnoredAtOpenClose()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(data);
			store.AddData(data);
			store.AddData(new StreamDataTest() { Offset = b.Length, Data = b });
			store.Close(new StreamDataTest() { Offset = b.Length, Data = b });
			Assert.True(b.Concat(b).SequenceEqual(ReadAllBytes(data.GetPath())));
		}

		[Fact]
		public void CanOpenAndAddTwice()
		{
			var b = new byte[] { 1 };
			store.Open(new StreamDataTest());
			store.AddData(new StreamDataTest() { Data = b });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			store.Close(new StreamDataTest() { Offset = 2 });
			Assert.True(Enumerable.Repeat((byte)1, 2).SequenceEqual(ReadAllBytes(new StreamDataTest().GetPath())));
		}

		[Fact]
		public void CanOpenButCannotAddLong()
		{
			var b = new byte[] { 1 };
			store.Open(new StreamDataTest());
			store.AddData(new StreamDataTest() { Data = b });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			Assert.Throws<TraceAssertException>(() => store.AddData(new StreamDataTest() { Data = b, Offset = 0 }));
		}

		[Fact]
		public void CanOpenButCannotAddShort()
		{
			var b = new byte[] { 1 };
			store.Open(new StreamDataTest());
			store.AddData(new StreamDataTest() { Data = b });
			Assert.Throws<TraceAssertException>(() => store.AddData(new StreamDataTest() { Data = b, Offset = 2 }));
		}

		[Fact]
		public void CanOpenAndAddTwiceThenUpdateSeveral()
		{
			var b = new byte[] { 1 };
			store.Open(new StreamDataTest());
			store.AddData(new StreamDataTest() { Data = b });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			store.AddData(new StreamDataTest() { Data = b, Offset = 1 });
			store.AddData(new StreamDataTest() { Data = b, Offset = 2 });
			store.Close(new StreamDataTest());
			Assert.True(Enumerable.Repeat((byte)1, 3).SequenceEqual(ReadAllBytes(new StreamDataTest().GetPath())));
		}

		[Fact]
		public void CanAddClosed()
		{
			var b = new byte[] { 1 };
			var data = new StreamDataTest() { Data = b };
			store.Open(new StreamDataTest());
			store.Close(new StreamDataTest());
			store.AddData(data);
			Assert.True(b.SequenceEqual(ReadAllBytes(data.GetPath())));
		}

		[Fact]
		public void CaAddWrittenClosed()
		{
			store.Open(new StreamDataTest());
			store.AddData(new StreamDataTest() { Data = new byte[] { 1 } });
			store.Close(new StreamDataTest());
			store.AddData(new StreamDataTest() { Data = new byte[] { 2 }, Offset = 1 });
			Assert.True(new byte[] { 1, 2 }.SequenceEqual(ReadAllBytes(new StreamDataTest().GetPath())));
		}

		[Fact]
		public void CannotAddWithoutOpen()
		{
			var b = new byte[] { 1 };
			Assert.Throws<FileNotFoundException>(() => store.AddData(new StreamDataTest() { Data = b }));
		}

		private static byte[] ReadAllBytes(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var res = new byte[stream.Length];
				var count = res.Length;
				var offset = 0;
				while (count > 0)
				{
					int read = stream.Read(res, offset, count);
					if (read == 0)
					{
						throw new Exception("EOF");
					}
					offset += read;
					count -= read;
				}
				return res;
			}
		}

		//everything is (should be) disposed properly but sometimes something might hold to the file (virus scanner ?)
		private void DeleteFiles()
		{
			foreach (var file in files)
			{
				var tries = 5;
				while (tries-- > 0)
				{
					try
					{
						if (File.Exists(file)) File.Delete(file);
						break;
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error: " + ex);
						if (tries == 0) throw;
						Thread.Sleep(1000);
					}
				}
			}
		}

		public void Dispose()
		{
			store.Dispose();
			DeleteFiles();
		}

		private class StreamDataTest : IStreamData
		{
			public int Offset { get; set; }
			public byte[] Data { get; set; }
			public string Path { get; set; }

			public StreamDataTest()
			{
				Path = files[0];
			}
			public string GetPath()
			{
				return Path;
			}
		}
	}
}
