namespace JCAutomation
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    public class ComponentWrapper : IComponent, IDisposable
    {
        private IDisposable disposable;

        public event EventHandler Disposed;

        public ComponentWrapper(IDisposable disposable)
        {
            this.disposable = disposable;
        }

        public void Dispose()
        {
            if (this.disposable != null)
            {
                this.disposable.Dispose();
            }
            this.disposable = null;
            EventHandler disposed = this.Disposed;
            if (disposed != null)
            {
                disposed(this, EventArgs.Empty);
            }
        }

        public ISite Site
        {
	        get
	        {
		        return
			        null;
	        }
	        set
            {
            }
        }
    }
}

