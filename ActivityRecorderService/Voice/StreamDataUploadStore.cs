using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService
{
	/// <summary>
	/// Class for coordinating data upload. For a given file method calls should not overlap.
	/// </summary>
	public class StreamDataUploadStore : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan maxIdleTime = TimeSpan.FromMinutes(60);
		private static readonly int cleanUpInterval = (int)maxIdleTime.TotalMilliseconds / 5;

		private readonly ConcurrentDictionary<string, TimestampedStream> streamDict = new ConcurrentDictionary<string, TimestampedStream>();
		private readonly Timer timer;

		public StreamDataUploadStore()
		{
			timer = new Timer(CleanUpIdleStreams);
			timer.Change(cleanUpInterval, cleanUpInterval);
		}

		public void Open(IStreamData data)
		{
			Open(data.GetPath());
		}

		public void Open(string path)
		{
			if (Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0) throw new ObjectDisposedException("StreamDataUploadStore");
			TimestampedStream stream;
			try
			{
				stream = GetStream(path, FileMode.CreateNew);
				if (!streamDict.TryAdd(path, stream)) //there should be no race here since calls to one path should not overlap
				{
					stream.Value.Dispose();
					ThrowError("Unexpected error while adding stream to dict at path " + path);
				}
			}
			catch (IOException ex)
			{
				if (Marshal.GetHRForException(ex) == -2147024816) //0x80070050 -- already exists
				{
					//probably confirmation to client is lost (that should be silently ignored)
					log.Debug("File already exists at path " + path);
					if (!streamDict.TryGetValue(path, out stream))
					{
						//probably server was restarted reload file
						stream = GetStream(path, FileMode.Open);
						//stream.Seek(0, SeekOrigin.End);
						if (!streamDict.TryAdd(path, stream)) //there should be no race here since calls to one path should not overlap
						{
							stream.Value.Dispose();
							ThrowError("Unexpected error while adding stream to dict at path " + path);
						}
					}
				}
				else
				{
					throw;
				}
			}
		}

		public void AddData(IStreamData data)
		{
			if (Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0) throw new ObjectDisposedException("StreamDataUploadStore");
			string path;
			Stream stream;
			GetPathAndStream(data, out path, out stream);
			AddDataToStream(stream, data.Data);
		}

		public void Close(IStreamData data)
		{
			Close(data.GetPath());
		}

		public void Close(string path)
		{
			if (Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0) throw new ObjectDisposedException("StreamDataUploadStore");
			CloseImpl(path);
		}

		public void CloseImpl(string path)
		{
			TimestampedStream stream;
			if (streamDict.TryRemove(path, out stream))
			{
				stream.Value.Dispose();
				log.Debug("Closed stream at path " + path);
			}
			else
			{
				log.Debug("Cannot close stream at path " + path);
			}
		}

		public void Delete(IStreamData data)
		{
			var path = data.GetPath();
			Close(path);
			Delete(path);
		}

		public void Delete(string path)
		{
			File.Delete(path);
			log.Debug("Deleted file at path " + path);
		}

		private void GetPathAndStream(IStreamData data, out string path, out Stream rawStream)
		{
			path = data.GetPath();
			TimestampedStream stream;
			if (!streamDict.TryGetValue(path, out stream))
			{
				//probably server was restarted reload file
				stream = GetStream(path, FileMode.Open); //todo FileNotFound should be sent to client as fault ? (other ThrowErrors?)
				//stream.Seek(0, SeekOrigin.End);
				if (!streamDict.TryAdd(path, stream)) //there should be no race here since calls to one path should not overlap
				{
					stream.Value.Dispose();
					ThrowError("Unexpected error while adding stream to dict at path " + path);
				}
			}
			rawStream = stream.Value;
			if (rawStream.Length < data.Offset)
			{
				ThrowError("Unexpected error stream is too small at path " + path);
			}
			else if (rawStream.Length > data.Offset + (data.Data == null ? 0 : data.Data.Length))
			{
				ThrowError("Unexpected error stream is too large at path " + path);
			}
			else if (rawStream.Position != data.Offset)
			{
				log.Debug("Repositioning stream from " + rawStream.Position + " to " + data.Offset);
				rawStream.Seek(data.Offset, SeekOrigin.Begin);
			}
		}

		private static void ThrowError(string errorMsg)
		{
			log.Error(errorMsg);
			Debug.Fail(errorMsg); //todo suppress in unit tests ?
			throw new Exception(errorMsg);
		}

		private static TimestampedStream GetStream(string path, FileMode mode)
		{
			return new TimestampedStream(new FileStream(path, mode, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough | FileOptions.SequentialScan));
		}

		private static void AddDataToStream(Stream stream, byte[] data)
		{
			if (data == null || data.Length == 0) return;
			stream.Write(data, 0, data.Length);
			stream.Flush();
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			timer.Change(Timeout.Infinite, Timeout.Infinite);
			timer.Dispose();
			foreach (var key in streamDict.Keys)
			{
				CloseImpl(key);
			}
		}

		private void CleanUpIdleStreams(object state)
		{
			if (Interlocked.CompareExchange(ref isDisposed, 0, 0) != 0) return;
			foreach (var key in streamDict.Keys)
			{
				TimestampedStream stream;
				if (streamDict.TryGetValue(key, out stream) && stream.GetLastAccess() > maxIdleTime)
				{
					log.Debug("Closing idle stream at path " + key);
					CloseImpl(key);
				}
			}
		}

		private class TimestampedStream
		{
			private readonly Stream stream;
			private int lastAccess;

			public Stream Value
			{
				get
				{
					Interlocked.Exchange(ref lastAccess, Environment.TickCount);
					return stream;
				}
			}

			public TimestampedStream(Stream stream)
			{
				if (stream == null) throw new ArgumentNullException("stream");
				this.stream = stream;
				lastAccess = Environment.TickCount;
			}

			public TimeSpan GetLastAccess()
			{
				return TimeSpan.FromMilliseconds((uint)(Environment.TickCount - lastAccess));
			}
		}
	}
}
