using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Common
{
	/// <summary>
	/// Asynchronous key based locking
	/// based on Stephen Cleary's post
	/// https://stackoverflow.com/a/31194647/2295648
	/// </summary>
	public sealed class AsyncDuplicateLock<TKey> where TKey : IEquatable<TKey>
	{
		private sealed class RefCounted<T>
		{
			public RefCounted(T value)
			{
				RefCount = 1;
				Value = value;
			}

			public int RefCount { get; set; }
			public T Value { get; private set; }
		}

		// Omit static to limit responsibility. You must instantiate a AsyncDuplicateLock locally for all callers
		private /* static */ readonly Dictionary<TKey, RefCounted<SemaphoreSlim>> SemaphoreSlims
			= new Dictionary<TKey, RefCounted<SemaphoreSlim>>();

		private SemaphoreSlim GetOrCreate(TKey key)
		{
			RefCounted<SemaphoreSlim> item;
			lock (SemaphoreSlims)
			{
				if (SemaphoreSlims.TryGetValue(key, out item))
				{
					++item.RefCount;
				}
				else
				{
					item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
					SemaphoreSlims[key] = item;
				}
			}
			return item.Value;
		}

		public IDisposable Lock(TKey key)
		{
			GetOrCreate(key).Wait();
			return new Releaser(this, key);
		}

		public async Task<IDisposable> LockAsync(TKey key)
		{
			await GetOrCreate(key).WaitAsync().ConfigureAwait(false);
			return new Releaser(this, key);
		}

		private sealed class Releaser : IDisposable
		{
			private readonly AsyncDuplicateLock<TKey> parent;
			private readonly TKey key;

			internal Releaser(AsyncDuplicateLock<TKey> parent, TKey key)
			{
				this.parent = parent;
				this.key = key;
			}

			public void Dispose()
			{
				RefCounted<SemaphoreSlim> item;
				lock (parent.SemaphoreSlims)
				{
					item = parent.SemaphoreSlims[key];
					--item.RefCount;
					if (item.RefCount == 0)
						parent.SemaphoreSlims.Remove(key);
				}
				item.Value.Release();
			}
		}
	}
}
