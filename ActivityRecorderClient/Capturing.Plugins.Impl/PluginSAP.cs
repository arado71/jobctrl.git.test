using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using JobCTRL.Plugins;
using log4net;
using sapfewse;
using saprotwr.net;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginSAP : ICaptureExtension
	{
		private class SAPConnector
		{
			private static volatile SAPConnector instance;
			private static readonly object lockObject = new object();
			private static CSapROTWrapper sapROTWrapper;
			private static object sapGuiRot;
			private static object sapEngine;
			private static GuiApplication sapGuiApp;

			private SAPConnector()
			{
				Create();
			}

			private static void Create()
			{
				sapROTWrapper = new CSapROTWrapper();
				sapGuiRot = sapROTWrapper.GetROTEntry("SAPGUI");
				sapEngine = sapGuiRot.GetType().InvokeMember("GetScriptingEngine", System.Reflection.BindingFlags.InvokeMethod, null, sapGuiRot, null);
				sapGuiApp = sapEngine as GuiApplication;
				if (sapGuiApp == null)
					throw new COMException("SAPConnector could not initialize the SAPGUI");
			}
			public static SAPConnector Instance
			{
				get
				{
					if (instance == null)
						lock (lockObject)
						{
							if (instance == null)
								instance = new SAPConnector();
						}
					return instance;
				}
			}
			public List<GuiSession> Sessions
			{
				get
				{
					lock (lockObject)
					{
						log.Debug("Getting sessions...");
						var sw = Stopwatch.StartNew();
						GuiComponentCollection connections;
						try
						{
							connections = sapGuiApp.Connections;
						}
						catch (COMException)
						{
							log.Debug("Recreating application object");
							Create();
							connections = sapGuiApp.Connections;
						}
						log.DebugFormat("Got connections. Took {0:N3} ms.", sw.Elapsed.TotalMilliseconds);
						sw.Restart();
						var castedConnections = connections.Cast<GuiConnection>();
						log.DebugFormat("Casted connections. Took {0:N3} ms.", sw.Elapsed.TotalMilliseconds);
						List<GuiSession> result = new List<GuiSession>();
						log.Debug("Enumerating through connectioncollection.");
						sw.Restart();
						foreach (var guiConnection in castedConnections)
						{
							foreach (var child in guiConnection.Children)
							{
								log.Debug("Casting guiConnection.Children to GuiSession.");
								var sw2 = Stopwatch.StartNew();
								var childAsGuiSession = (GuiSession) child;
								log.DebugFormat("Casting took {0:N3} ms.", sw2.Elapsed.TotalMilliseconds);
								sw2.Restart();
								var info = childAsGuiSession.Info;
								log.DebugFormat("Got GuiSessionInfo. Took {0:N3} ms.", sw2.Elapsed.TotalMilliseconds);
								sw2.Restart();
								if (!string.IsNullOrEmpty(info.User))
								{
									result.Add(childAsGuiSession);
								}
								log.DebugFormat("Filtered on info.User. Took {0:N3} ms.", sw2.Elapsed.TotalMilliseconds);
							}
						}
						log.DebugFormat("Enumeration took {0:N3} ms. Got {1} element.", sw.Elapsed.TotalMilliseconds, result.Count);
						return result;
					}
				}
			}
		}
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IEnumerable<KeyValuePair<string, string>> EmptyResult = Enumerable.Empty<KeyValuePair<string, string>>();
		private readonly CachedDictionary<IntPtr, List<KeyValuePair<string, string>>> res = new CachedDictionary<IntPtr, List<KeyValuePair<string, string>>>(TimeSpan.FromSeconds(1), true);

		private const string PluginId = "JobCTRL.SAP";
		private const string ParamProcess = "ProcessName";
		private const string ParamCapture = "Capture";
		private const string ParamSeparator = "Separator";
		private const string ParamTitle = "Title";

		private string titleExpr, oldTitle = String.Empty;
		private string separator = ":";
		private string savedParamValue;
		private HashSet<string> processNamesToCheck = new HashSet<string> { "saplogon.exe" };
		private List<KeyValuePair<string, string>> captures = new List<KeyValuePair<string, string>>();
		private readonly CachedFunc<List<GuiSession>> sessionCache;
		private readonly bool isVerboseLogging;
		public string Id
		{
			get { return PluginId; }
		}

		public PluginSAP()
		{
			isVerboseLogging = log.Logger.IsEnabledFor(log4net.Core.Level.Verbose);
			sessionCache = new CachedFunc<List<GuiSession>>(GetSessions, TimeSpan.FromMinutes(1));
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcess;
			yield return ParamCapture;
			yield return ParamSeparator;
			yield return ParamTitle;
		}
		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamTitle))
				titleExpr = value;
			if (string.Equals(name, ParamSeparator))
			{
				separator = value;
				if (!string.IsNullOrEmpty(savedParamValue))
					compile(savedParamValue);
			}
			if (string.Equals(name, ParamProcess, StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrEmpty(value))
					processNamesToCheck = null;
				else
				{
					processNamesToCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (var file in value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
						processNamesToCheck.Add(file);
				}
			}
			if (string.Equals(name, ParamCapture, StringComparison.OrdinalIgnoreCase))
			{
				compile(value);
				savedParamValue = value;	// for might upcoming separator 
			}
		}
		private void compile(string value)
		{
			try
			{
				captures = Compile(value);
			}
			catch (Exception ex)
			{
				log.Warn("Failed to compile script", ex);
				captures = null;
			}
		}
		private List<KeyValuePair<string, string>> Compile(string value)
		{
			List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
			if (string.IsNullOrEmpty(value)) throw new Exception("Script cannot be empty");
			string[] pairs = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var pair in pairs)
			{
				string[] kv = pair.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
				if (kv.Length == 2)
					result.Add(new KeyValuePair<string, string>(kv[0].Trim(), kv[1].Trim()));
				else
					log.ErrorFormat("Error compiling SAP GUI field path (separator char is `" + separator + "`)", new ArgumentException(value));
			}
			return result;
		}
		public IEnumerable<string> GetCapturableKeys()
		{
			return captures.Select(x => x.Key);
		}
		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string callerProcessName)
		{
			if (captures == null || captures.Count == 0) return EmptyResult;
			if (processNamesToCheck != null && !processNamesToCheck.Contains(callerProcessName))
				return EmptyResult;

			if (ElevatedPrivilegesHelper.IsElevated)
			{
				log.Warn("jCTRL runs as Admin");
				return EmptyResult;
			}

			// filter out fake windows
			var classNameStringBuilder = new StringBuilder(256);
			var length = WinApi.GetClassName(hWnd, classNameStringBuilder, classNameStringBuilder.Capacity);
			if (length > 0 && !classNameStringBuilder.ToString().StartsWith("SAP_FRONTEND_SESSION"))
				return EmptyResult;
			List<KeyValuePair<string, string>> ret;
			Stopwatch sw = Stopwatch.StartNew();
			if (isVerboseLogging)
				log.VerboseFormat("Measurement started, hWnd:{0:x}, pid:{1}, captures: {2}", hWnd.ToInt32(), processId, string.Join(", ", captures.Select(c => string.Format("[Key: {0}, Value: {1}]", c.Key, c.Value)).ToArray()));
			try
			{
				var sessions = sessionCache.GetOrCalculateValue();
				if (isVerboseLogging)
					log.VerboseFormat("After getSession +{0}ms", sw.ElapsedMilliseconds);
				if (sessions == null)
				{
					if (isVerboseLogging)
						log.VerboseFormat("No session, exited +{0}ms", sw.ElapsedMilliseconds);
					return EmptyResult;
				}

				var session = sessions.FirstOrDefault(f => f.IsActive);
				if (session == null)
				{
					if (isVerboseLogging)
						log.VerboseFormat("No active session found, exited +{0}ms", sw.ElapsedMilliseconds);
					res.TryGetValue(hWnd, out ret);
					return ret;
				}

				var activeWindow = session.ActiveWindow;
				var activeWindowHandle = activeWindow.Handle;
				if (activeWindowHandle != hWnd.ToInt32())
				{
					if (isVerboseLogging)
						log.VerboseFormat("Not the active window({0:x}), exited +{1}ms", activeWindowHandle, sw.ElapsedMilliseconds);
					res.TryGetValue(hWnd, out ret);
					return ret;
				}

				var activeWindowText = activeWindow.Text;
				if (titleExpr != null && oldTitle != activeWindowText)
				{
					var rx = new Regex(titleExpr, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
					if (!rx.Match(activeWindowText).Success)
					{
						if (isVerboseLogging)
							log.VerboseFormat("Window title filter not matched, title:{0}, filter:{1}, exited +{2}ms", activeWindowText,
								titleExpr, sw.ElapsedMilliseconds);
						oldTitle = string.Empty;
						res.TryGetValue(hWnd, out ret);
						return ret;
					}
					log.DebugFormat("SAP window title is `{0}`", activeWindowText);
					oldTitle = activeWindowText;
				}
				res.Clear();
				if (isVerboseLogging)
					log.Verbose("Attach to SAP session done succesfully, captures=" + captures.Count);
				foreach (var capture in captures)
				{
					string controlPath;
					if (!capture.Value.StartsWith("/app/"))
						controlPath = session.Id + "/" + capture.Value;
					else
					{
						var shortPath = capture.Value.Substring(capture.Value.IndexOf("wnd", StringComparison.InvariantCulture));
						controlPath = session.Id + "/" + shortPath;
					}

					bool tryAgain = true;
					for (int i = 0; i < 2 && tryAgain; i++)
					{
						try
						{
							dynamic d = session.FindById(controlPath);
							var captureResult = d.Text as string;
							if (captureResult == null) continue;

							if (res.TryGetValue(hWnd, out ret))
							{
								if (ret.Any(e => e.Key == capture.Key))
								{
									var item = ret.First(e => e.Key == capture.Key);
									var kvp = new KeyValuePair<string, string>(capture.Key, item.Value + "," + captureResult);
									ret.Remove(item);
									ret.Add(kvp);
								}
								else
									ret.Add(new KeyValuePair<string, string>(capture.Key, captureResult));
							}
							else
								res.Add(hWnd, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(capture.Key, captureResult) });

							log.DebugFormat("AutomationCapture capturing {0} was {1}", capture.Key, captureResult);
							tryAgain = false;
							break;
						}
						catch (COMException ce)
						{
							if (ce.ErrorCode == 0x026B) // 619:field not found
							{
								log.ErrorFormat("Field not found, ControlPath=" + controlPath);
								sessions = sessionCache.CalculateValue();
								if (isVerboseLogging)
									log.VerboseFormat("Session refreshed +{0}ms", sw.ElapsedMilliseconds);
								session = sessions.FirstOrDefault(f => f.IsActive);
								if (session == null)
								{
									if (isVerboseLogging)
										log.VerboseFormat("No active session found, exited +{0}ms", sw.ElapsedMilliseconds);
									return EmptyResult;
								}
							}
							else
							{
								log.Error("Capturing " + capture.Key, ce);
								tryAgain = false;
							}
						}
						catch (Exception ex)
						{
							log.Error("Capturing (inner)", ex);
							tryAgain = false;
						}
					}
				}
			}
			catch (COMException ce)
			{
				if (ce.ErrorCode != 0x800706BA) throw;
				log.Error("Capturing", ce);
				return null;
			}
			catch (Exception ex)
			{
				log.Error("Capturing", ex);
			}
			log.Verbose("Capturing Time elapsed in ms:" + sw.ElapsedMilliseconds);
			res.TryGetValue(hWnd, out ret);
			return ret;
		}
		private List<GuiSession> GetSessions()
		{
			Stopwatch sw = null;
			if (isVerboseLogging)
				sw = Stopwatch.StartNew();
			var session = SAPConnector.Instance.Sessions;
			if (isVerboseLogging && sw != null)
				log.VerboseFormat("GetSessions took {0} ms", sw.ElapsedMilliseconds);
			return session;
		}
	}
}
