using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;

namespace Reporter
{
	public static class Profiler
	{
		private static readonly bool canRun;
		private static readonly ThreadLocal<ProfilingSession> sessions = new ThreadLocal<ProfilingSession>(() => new ProfilingSession()); 
		static Profiler()
		{
			XmlConfigurator.Configure();
			canRun = bool.TryParse(ConfigurationManager.AppSettings["Profiling"], out canRun) && canRun;
		}

		public static IDisposable Measure([System.Runtime.CompilerServices.CallerMemberName] string name = "?")
		{
			if (!canRun) return null;
			return sessions.Value.StartMeasure(name);
		}

		private class ProfilingSession
		{
			private static readonly ILog log = LogManager.GetLogger(typeof(ProfilingSession));
			private readonly Stack<ProfilerTimer> activeTimers = new Stack<ProfilerTimer>(); 
			private readonly Stopwatch overheadTimer = new Stopwatch();
			public IDisposable StartMeasure(string name)
			{
				if (!canRun) return null;
				overheadTimer.Start();
				ProfilerTimer parent = null;
				if (activeTimers.Count > 0)
				{
					parent = activeTimers.Peek();
				}

				var context = new ProfilerTimer(this, parent, name);
				activeTimers.Push(context);
				context.Start();
				overheadTimer.Stop();
				return context;
			}

			private void TimerFinished()
			{
				overheadTimer.Start();
				var lastContext = activeTimers.Pop();
				if (activeTimers.Count == 0)
				{
					var sb = new StringBuilder(1024);
					sb.AppendLine("Profiling:");
					lastContext.Evaluate(sb);
					var overhead = overheadTimer.Elapsed.TotalMilliseconds;
					sb.Append("Profiling overhead was " + overhead + " ms");
					overheadTimer.Reset();
					log.Debug(sb.ToString());
					return;
				}

				overheadTimer.Stop();
			}

			private class ProfilerTimer : IDisposable
			{
				private readonly string name;
				private TimeSpan elapsed;
				private Stopwatch stopwatch;
				private readonly ProfilingSession session;
				private readonly List<ProfilerTimer> children = new List<ProfilerTimer>();

				public ProfilerTimer(ProfilingSession session, ProfilerTimer parent, string name)
				{
					this.name = name;
					this.session = session;
					if (parent != null) parent.AddChildren(this);
				}

				public void Start()
				{
					stopwatch = Stopwatch.StartNew();
				}

				public void Evaluate(StringBuilder sb, int depth = 0)
				{
					sb.Append(' ', depth).Append(name).Append(" took ").Append(elapsed.TotalMilliseconds).Append(" ms").AppendLine();
					foreach (var child in children)
					{
						child.Evaluate(sb, depth + 1);
					}
				}

				public void Dispose()
				{
					elapsed = stopwatch.Elapsed;
					session.TimerFinished();
				}

				private void AddChildren(ProfilerTimer child)
				{
					children.Add(child);
				}
			}
		}
	}
}
