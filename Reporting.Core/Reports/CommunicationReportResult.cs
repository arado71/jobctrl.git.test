using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Reports
{
	public class CommunicationReportResult
	{
		public List<IUser> Users { get; set; }
		public List<ICommunicationAmount> Inbound { get; set; }
		public List<ICommunicationAmount> Outbound { get; set; }

		public void SerializeTo(out string users, out string communicationMatrix)
		{
			var sb = new StringBuilder();
			sb.AppendLine("name,color");
			foreach (var reportableUser in Users)
			{
				var name = reportableUser.Name.Replace(",", " ");
				sb.Append(name).Append(",#").Append((Math.Abs(name.GetHashCode()) % ((int)Math.Pow(256, 3))).ToString("X6"));
				sb.AppendLine();
			}

			users = sb.ToString();
			sb.Clear();

			sb.Append("{");
			if (Inbound != null && Inbound.Count > 0)
			{
				sb.Append("\"in\": ");
				AppendMatrix(sb, Users, Inbound);
				sb.Append(", ");
			}
			sb.Append("\"out\": ");
			AppendMatrix(sb, Users, Outbound);
			sb.Append("}");

			communicationMatrix = sb.ToString();
		}

		private static Dictionary<Tuple<int, int>, TimeSpan> GetLookup(List<ICommunicationAmount> commAmounts)
		{
			var lookup = new Dictionary<Tuple<int, int>, TimeSpan>();
			if (commAmounts != null)
			{
				foreach (var communicationAmount in commAmounts)
				{
					var key = Tuple.Create(communicationAmount.From.UserId, communicationAmount.To.UserId);
					TimeSpan orig;
					if (!lookup.TryGetValue(key, out orig))
					{
						orig = TimeSpan.Zero;
					}
					lookup[key] = orig + communicationAmount.Duration;
				}
			}
			return lookup;
		}

		private static void AppendMatrix(StringBuilder sb, List<IUser> users, List<ICommunicationAmount> commAmounts)
		{
			var lookup = GetLookup(commAmounts);
			sb.Append("[");
			var isFirstPar = true;
			foreach (var from in users)
			{
				sb.Append(isFirstPar ? "[" : ", [");
				isFirstPar = false;
				var isFirst = true;
				foreach (var to in users)
				{
					TimeSpan currTime;
					if (!lookup.TryGetValue(Tuple.Create(from.UserId, to.UserId), out currTime))
					{
						currTime = TimeSpan.Zero;
					}
					sb.Append(isFirst ? "" : ", ").Append(currTime.TotalSeconds.ToString(CultureInfo.InvariantCulture));
					isFirst = false;
				}
				sb.AppendLine("]");
			}
			sb.Append("]");
		}
	}
}
