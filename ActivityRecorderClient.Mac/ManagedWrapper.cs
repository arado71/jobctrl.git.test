using System;
using System.Runtime.InteropServices;

namespace Tct.ActivityRecorderClient
{
	public abstract class ManagedWrapper : IDisposable
	{
		public IntPtr Handle { get; private set; }

		private bool disposed;
		
		protected ManagedWrapper(IntPtr handle, bool owns)
		{
			Handle = handle;
			if (!owns)
				Retain(Handle);
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		~ManagedWrapper()
		{
			Dispose(false);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			System.Diagnostics.Debug.Assert(disposing, "Forgot to Dispose instance");
			System.Diagnostics.Debug.Assert(!disposed, "Already Disposed");
			if (disposed)
				return;
			disposed = true; //avoid cycles, so one cannot call Dispose from Release
			if (Handle == IntPtr.Zero)
				return;
			Release(Handle);
			Handle = IntPtr.Zero;
		}
		
		protected abstract void Retain(IntPtr handle);

		protected abstract void Release(IntPtr handle);
	}
	
	public class CFWrapper : ManagedWrapper
	{
		protected CFWrapper(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}
		
		public static CFWrapper CreateOwned(IntPtr handle)
		{
			return new CFWrapper(handle, true);
		}
		
		protected override void Retain(IntPtr handle)
		{
			CFRetain(handle);
		}

		protected override void Release(IntPtr handle)
		{
			CFRelease(handle);
		}

		[DllImport(LibraryMac.CoreFundation.Path)]
		private static extern void CFRelease(IntPtr handle);

		[DllImport(LibraryMac.CoreFundation.Path)]
		private static extern void CFRetain(IntPtr handle);
	}
}

