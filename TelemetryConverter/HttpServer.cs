using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelemetryConverter
{
	public class HttpServer : IDisposable
	{
		private readonly string baseUrl;
		private readonly Dictionary<string, Action<HttpListenerContext>> routes = new Dictionary<string, Action<HttpListenerContext>>();
		private readonly CancellationTokenSource cts = new CancellationTokenSource();
		private readonly object thisLock = new object();
		private Thread serverThread;

		public HttpServer(string baseUrl)
		{
			this.baseUrl = baseUrl.TrimEnd('/');
		}

		public void Add(string url, Action<HttpListenerContext> callback)
		{
			routes.Add(url, callback);
		}

		public void Start()
		{
			lock (thisLock)
			{
				serverThread = new Thread(() => HttpServerJob(cts.Token)) { IsBackground = true };
				serverThread.Start();
			}
		}

		private void HttpServerJob(CancellationToken ct)
		{
			var httpServer = new HttpListener();
			foreach (var route in routes)
			{
				httpServer.Prefixes.Add(baseUrl + route.Key);
			};
			httpServer.Start();
			while (!ct.IsCancellationRequested)
			{
				var asyncReq = httpServer.GetContextAsync();
				try
				{
					asyncReq.Wait(ct);
				}
				catch (OperationCanceledException)
				{
					return;
				}

				if (asyncReq.IsCanceled) return;
				var context = asyncReq.Result;
				ServeRequest(context);
			}
		}

		private void ServeRequest(HttpListenerContext context)
		{
			var found = false;
			if (context.Request.HttpMethod == "OPTIONS")
			{
				Console.WriteLine("Options received!");
				context.Response.StatusCode = 200;
				context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
				context.Response.Headers.Add("Access-Control-Allow-Headers", "Accept, Content-Type");
				context.Response.Headers.Add("Allow", "POST");
				context.Response.OutputStream.Close();
				return;
			}

			foreach (var route in routes)
			{
				if (string.Equals(context.Request.RawUrl.TrimEnd('/'), route.Key.TrimEnd('/')))
				{
					try
					{
						context.Response.StatusCode = 200;
						context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
						context.Response.Headers.Add("Access-Control-Allow-Methods", "POST");
						route.Value(context);
						Console.WriteLine("Web query: " + context.Request.RawUrl);
					}
					catch (Exception ex)
					{

						context.Response.StatusCode = 500;
						context.SetResponse("<b>Exception occured while processing:</b><br />" + ex.Message.Replace("\n", "<br />"));

					}
					found = true;
					break;
				}
			}

			if (!found)
			{
				using (var stream = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
				{
					context.Response.StatusCode = 404;
					stream.Write("Page not found");
					//Console.WriteLine("Web error, no page " + context.Request.RawUrl);
				}
			}
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				cts.Cancel();
				if (serverThread != null) serverThread.Join();
			}
		}
	}
}
