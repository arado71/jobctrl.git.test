using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Menu
{
	public class Versionable<T>
	{
		public T Value { get; private set; }
		public long Version { get; private set; }
		public event EventHandler Changed;

		public Versionable()
		{
			Version = 0;
		}

		public void Update(T value)
		{
			Value = value;
			Version = unchecked(Version + 1);
			var evt = Changed;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		public IDisposable UpdateWithDeferredEvent(T value)
		{
			Value = value;
			Version = unchecked(Version + 1);
			return new DeferredEventRaiser(this);
		}

		private class DeferredEventRaiser : IDisposable
		{
			private readonly Versionable<T> parent;

			public DeferredEventRaiser(Versionable<T> parent)
			{
				this.parent = parent;
			}

			public void Dispose()
			{
				var del = parent.Changed;
				if (del != null) del(parent, EventArgs.Empty);
			}
		}
	}
}
