using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.View
{
	public class ComponentWrapper : IComponent
	{
		private IDisposable disposable;

		public ComponentWrapper(IDisposable disposable)
		{
			this.disposable = disposable;
		}

		#region IComponent Members

		public event EventHandler Disposed;

		public ISite Site
		{
			get
			{
				return null;
			}
			set
			{
				//do nothing
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
			disposable = null;
			EventHandler disposed = Disposed;
			if (disposed != null) disposed(this, EventArgs.Empty);
		}

		#endregion
	}
}
