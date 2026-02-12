using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.Grafana
{
	public class GrafanaHttpServer : IDisposable
	{
		private readonly DataProcessor processor;
		private readonly HttpServer server;

		public GrafanaHttpServer(string baseUrl, DataProcessor dataSource)
		{
			server = new HttpServer(baseUrl);
			processor = dataSource;
		}

		public void Start()
		{
			foreach (var category in processor.Categories)
			{
				RegisterRoutes(category);
			}

			server.Start();
		}

		private void RegisterRoutes(string category)
		{
			server.Add(string.Format("/{0}/", category), RespondOk);
			server.Add(string.Format("/{0}/search/", category), RespondNames);
			server.Add(string.Format("/{0}/query/", category), RespondQuery);
			server.Add(string.Format("/{0}/annotations/", category), RespondAnnotations);
		}

		private static void RespondOk(HttpListenerContext context)
		{
			context.SetResponse("Ok");
		}

		private void RespondNames(HttpListenerContext context)
		{
			var category = GetCategory(context);
			var ret = processor.ListAggregatorNames(category).ToList();
			ret.AddRange(new string[]
			{	
				"Feature|{\"Action\":\"StartStop\",\"Name\":\"CurrentWork\"}"
				,"Feature|{\"Action\":\"StartMeeting\",\"Name\":\"Hotkey\"}"
				,"Feature|{\"Action\":\"StartLock\",\"Name\":\"Meeting\"}"
				,"Feature|{\"Action\":\"Open\",\"Name\":\"MainMenu\"}"
				,"Feature|{\"Action\":\"Close\",\"Name\":\"MainMenu\"}"
				,"Feature|{\"Action\":\"StartAdhoc\",\"Name\":\"Meeting\"}"
				,"Feature|{\"Action\":\"Clicked\",\"Name\":\"Exit\"}"
				,"Feature|{\"Action\":\"Click\",\"Name\":\"Notification\"}"
				,"Feature|{\"Action\":\"Open\",\"Name\":\"Settings\"}"
				,"Feature|{\"Action\":\"Close\",\"Name\":\"Settings\"}"
				,"Feature|{\"Action\":\"Close\",\"Name\":\"Notification\"}"
			});
			context.SetJsonResponse(ret);
		}

		private void RespondQuery(HttpListenerContext context)
		{
			var req = context.GetRequestJson();
			var from = (DateTime)req["range"]["from"];
			var to = (DateTime)req["range"]["to"];
			var interval = GrafanaHelper.ConvertTimespan((string)req["interval"]);
			var names = new List<string>();
			foreach (var target in req["targets"])
			{
				var queryName = (string)target["target"];
				names.Add(queryName);
			}

			var category = GetCategory(context);
			var result = new List<object>();
			result.AddRange(processor.GetSeriesAggregate(category, names, from, to, interval));
			result.AddRange(processor.GetTableAggregate(category, names, from, to, interval));

			context.SetJsonResponse(result);
		}

		private void RespondAnnotations(HttpListenerContext context)
		{
			context.SetJsonResponse(new object[] { });
		}

		private static string GetCategory(HttpListenerContext context)
		{
			return context.Request.RawUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
		}

		public void Dispose()
		{
			server.Dispose();
		}
	}
}
