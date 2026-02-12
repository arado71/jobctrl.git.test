using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public class CachingCaptureExtensionWrapper : ICaptureExtension, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Func<IntPtr, string> KeySelectorHwnd = n => n.ToString();
		public static readonly Func<IntPtr, string> KeySelectorTitleAndHwnd = n => n.ToString() + "_" + WindowTextHelper.GetWindowText(n);

		private readonly CachedDictionary<string, IEnumerable<KeyValuePair<string, string>>> cachedResultsForErrors;
		private readonly CachedDictionary<string, IEnumerable<KeyValuePair<string, string>>> cachedResultsForSuccess;
		private readonly ICaptureExtension inner;
		private readonly Func<IntPtr, string> keySelector;

		public CachingCaptureExtensionWrapper(Func<ICaptureExtension> factoryFunc, TimeSpan durationForSuccess, TimeSpan durationForErrors, Func<IntPtr, string> keySelector)
		{
			if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
			inner = factoryFunc();
			if (inner == null) throw new InvalidOperationException("factoryFunc returned null");
			this.keySelector = keySelector ?? KeySelectorHwnd;
			if (durationForErrors > TimeSpan.Zero)
			{
				cachedResultsForErrors = new CachedDictionary<string, IEnumerable<KeyValuePair<string, string>>>(durationForErrors, true);
			}
			if (durationForSuccess > TimeSpan.Zero)
			{
				cachedResultsForSuccess = new CachedDictionary<string, IEnumerable<KeyValuePair<string, string>>>(durationForSuccess, true);
			}
		}

		public string Id
		{
			get { return inner.Id; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			return inner.GetParameterNames();
		}

		public void SetParameter(string name, string value)
		{
			inner.SetParameter(name, value);
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			return inner.GetCapturableKeys();
		}

		// ReSharper disable PossibleMultipleEnumeration
		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			IEnumerable<KeyValuePair<string, string>> result;
			var key = keySelector(hWnd);
			if (cachedResultsForSuccess != null
				&& cachedResultsForSuccess.TryGetValue(key, out result))
			{
				return result;
			}
			result = inner.Capture(hWnd, processId, processName);
			if (result != null)
			{
				result = result is ICollection<KeyValuePair<string, string>> //make sure result is not deferred
					? result
					: result.ToArray();
				if (cachedResultsForErrors != null) cachedResultsForErrors.Set(key, result);
				if (cachedResultsForSuccess != null) cachedResultsForSuccess.Set(key, result);
			}
			else //result == null
			{
				//log.Verbose($"Couldn't find cached value for capture (type: {inner.GetType().FullName}). Caching null value.");
				cachedResultsForErrors.TryGetValue(key, out result);
				cachedResultsForSuccess?.Set(key, result);
			}
			return result;
		}
		// ReSharper restore PossibleMultipleEnumeration

		public void Dispose()
		{
			using (inner as IDisposable) { } //dispose inner
		}
	}
}
