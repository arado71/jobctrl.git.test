using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class ParallelCollection<TIn> : IDisposable where TIn : class
	{
		private readonly List<Entry<TIn>> collection = new List<Entry<TIn>>();
		private readonly AutoResetEvent processingCompleted = new AutoResetEvent(false);
		private readonly Func<TIn, string> nameSelector;
		private readonly IEqualityComparer<TIn> comparer;
		private readonly TimeSpan disposeTimeout;
		private readonly object thisLock = new object();
		private int currentlyProcessing;

		public TIn this[int index]
		{
			get
			{
				return collection[index].Object;
			}
		}

		public ParallelCollection(IEnumerable<TIn> baseObjects, TimeSpan disposeTimeout, Func<TIn, string> nameSelector = null, IEqualityComparer<TIn> comparer = null)
		{
			this.comparer = comparer ?? EqualityComparer<TIn>.Default;
			this.nameSelector = nameSelector;
			this.disposeTimeout = disposeTimeout;
			Add(baseObjects);
		}

		public void Clear()
		{
			lock (thisLock)
			{
				foreach (var threadObj in collection.Select(x => x.Task))
				{
					threadObj.Dispose();
				}

				collection.Clear();
			}
		}

		private void Add(IEnumerable<TIn> baseObjects)
		{
			if (baseObjects == null) return;
			lock (thisLock)
			{
				foreach (var baseObject in baseObjects)
				{
					Add(baseObject);
				}
			}
		}

		private void Add(TIn addedObject)
		{
			// Assert thisLock is held
			string name = null;
			if (nameSelector != null)
			{
				name = nameSelector(addedObject);
			}

			var task = new StaTask(name) { StopTimeout = disposeTimeout };
			task.ProcessingCompleted += HandleProcessCompleted;
			collection.Add(new Entry<TIn> { Object = addedObject, Task = task });
		}

		private int Find(TIn targetObject)
		{
			// Assert thisLock is held
			for (var i = 0; i < collection.Count; ++i)
			{
				if (comparer.Equals(collection[i].Object, targetObject)) return i;
			}

			return -1;
		}

		private void RemoveAt(int index)
		{
			// Assert thisLock is held
			collection[index].Task.ProcessingCompleted -= HandleProcessCompleted;
			collection[index].Task.Dispose();
			collection.RemoveAt(index);
		}

		private void Remove(TIn removedObject)
		{
			// Assert thisLock is held
			var i = Find(removedObject);
			if (i != -1) RemoveAt(i);
		}

		public void Set(IEnumerable<TIn> newObjects)
		{
			lock (thisLock)
			{
				var removedObjects = new HashSet<TIn>(collection.Select(x => x.Object), comparer);
				var createdObjects = new HashSet<TIn>(newObjects, comparer);
				foreach (var removed in removedObjects.Except(createdObjects))
				{
					Remove(removed);
				}

				foreach (var added in createdObjects.Except(removedObjects))
				{
					Add(added);
				}
			}
		}

		public TResult[] MapOrDefault<TResult>(Func<TIn, TResult> functionCall, TimeSpan timeout)
		{
			lock (thisLock)
			{
				Interlocked.Exchange(ref currentlyProcessing, 0);
				foreach (var entry in collection)
				{
					var baseObject = entry.Object;
					var couldStart = entry.Task.BeginInvoke(() => functionCall(baseObject));
					if (couldStart)
					{
						Interlocked.Increment(ref currentlyProcessing);
					}
				}

				processingCompleted.Reset();
				if (Interlocked.CompareExchange(ref currentlyProcessing, 0, 0) > 0)
				{
					processingCompleted.WaitOne(timeout);
				}

				var result = new TResult[collection.Count];
				var i = 0;
				foreach (var entry in collection)
				{
					TResult callResult;
					if (!entry.Task.TryEndInvoke(out callResult))
					{
						entry.FailCount++;
					}
					else
					{
						entry.FailCount = 0;
					}

					result[i++] = callResult;
				}

				return result;
			}
		}

		public TIn[] GetFailed(int minFails = 1)
		{
			lock (thisLock)
			{
				return collection.Where(x => x.FailCount >= minFails).Select(x => x.Object).ToArray();
			}
		}

		private void HandleProcessCompleted(object sender, SingleValueEventArgs<object> e)
		{
			if (Interlocked.Decrement(ref currentlyProcessing) == 0)
			{
				processingCompleted.Set();
			}
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				processingCompleted.Close();
				Clear();
			}
		}

		private class Entry<T>
		{
			public T Object { get; set; }
			public StaTask Task { get; set; }
			public int FailCount { get; set; }
		}
	}
}
