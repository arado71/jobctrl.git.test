using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using log4net;

namespace JiraSyncTool.Jira.Utils
{
	public static class WcfHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly TimeSpan MaxRetryTime =TimeSpan.FromMinutes(1);
		private static readonly TimeSpan StartRetryTime = TimeSpan.FromSeconds(2);
		private const int MaxRetryCount = 10;

		/// <summary>
		/// Calls the provided delegate with retry logic on timeout, with elapsed time measurement.
		/// </summary>
		/// <typeparam name="TBase"></typeparam>
		/// <typeparam name="TRet"></typeparam>
		/// <param name="proxy"></param>
		/// <param name="call"></param>
		/// <param name="callName"></param>
		/// <returns>The result of the delegate call.</returns>
		/// <remarks>The timeout retry algorithm uses exponential backoff, doubling the retry interval after each unsuccessful try up until a certain threshold.</remarks>
		public static TRet Execute<TBase, TRet>(this TBase proxy, Func<TBase, TRet> call, string callName) where TBase : System.Web.Services.Protocols.SoapHttpClientProtocol
		{
			var retryTime = StartRetryTime;
			var retryCount = 0;
			while (true)
			{
				var sw = Stopwatch.StartNew();
				try
				{
					return call(proxy);
				}
				catch (TimeoutException)
				{
					++retryCount;
					if (retryCount > MaxRetryCount) throw new TimeoutException(callName + " call timed out " + retryCount + " times");
					log.Debug(callName + " call timed out, waiting " + retryTime + "ms before retry");
					Thread.Sleep(retryTime);
					retryTime = new TimeSpan(Math.Min(retryTime.Ticks*2, MaxRetryTime.Ticks));
				}
				catch (FaultException ex)
				{
					log.Error(callName + "call failed with FaultException: " + ex.Message, ex);
					throw;
				}
				catch (Exception ex)
				{
					log.Error(callName + " call failed", ex);
					throw;
				}
				finally
				{
					log.DebugFormat(callName + " returned in {0} ms", sw.Elapsed.TotalMilliseconds);
				}
			}
		}

		public static void Execute<TBase>(this TBase proxy, Action<TBase> call, string callName) where TBase : System.Web.Services.Protocols.SoapHttpClientProtocol
		{
			Execute(proxy, n =>
			{
				call(n);
				return true;
			}, callName);
		}
	}
}
