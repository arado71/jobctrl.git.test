using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.CustomReporting
{
	public class AppDomainWrapper : IDisposable
	{
		private AppDomain executeDomain;
		private AppDomainWrapper(string name)
		{
			var appDomainSetup = new AppDomainSetup();
			string appBase = Assembly.GetExecutingAssembly().CodeBase;
			appBase = new Uri(appBase).LocalPath;
			appDomainSetup.ApplicationBase = Path.GetDirectoryName(appBase);
			var evidence = AppDomain.CurrentDomain.Evidence;
			executeDomain = AppDomain.CreateDomain(name, evidence, appDomainSetup);
		}

		public static AppDomainWrapper CreateDomain(string name)
		{
			return new AppDomainWrapper(name);
		}

		public T CreateInstanceAndUnwrap<T>(params object[] args) where T : MarshalByRefObject
		{
			Contract.Requires<ObjectDisposedException>(executeDomain != null);

			return (T)executeDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, true, 0, null, args, null, null);
		}

		public void Dispose()
		{
			Contract.Requires<ObjectDisposedException>(executeDomain != null);

			if (executeDomain != null)
			{
				AppDomain.Unload(executeDomain);
				executeDomain = null;
			}
		}
	}
}
