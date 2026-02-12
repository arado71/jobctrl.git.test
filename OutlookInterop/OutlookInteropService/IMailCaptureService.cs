using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace OutlookInteropService
{
	[ServiceContract]
	public interface IMailCaptureService
	{
		[OperationContract]
		MailCaptures GetMailCaptures();

		[OperationContract]
		void StopService();
	}

	[DataContract(Namespace = "http://jobctrl.com/mail", Name = "MailCaptures")]
	public class MailCaptures
	{
		[DataMember(Order = 1)]
		public Dictionary<int, MailCapture> MailCaptureByHWnd { get; set; }

		[DataMember(Order = 2)]
		public bool IsSafeMailItemCommitUsable { get; set; }

		public override string ToString()
		{
			if (MailCaptureByHWnd == null) return "MailCaptures (null)";
			var sb = new StringBuilder();
			sb.Append("MailCaptures ");
			foreach (var kvp in MailCaptureByHWnd)
			{
				sb.Append("{ ")
					.Append(kvp.Key)
					.Append(", ")
					.Append(kvp.Value)
					.Append(" }");
			}
			return sb.ToString();
		}
	}

	[DataContract(Namespace = "http://jobctrl.com/mail", Name = "MailCapture")]
	public class MailCapture
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public MailAddress From { get; set; }

		[DataMember]
		public List<MailAddress> To { get; set; }

		[DataMember]
		public List<MailAddress> Cc { get; set; }

		[DataMember]
		public string Subject { get; set; }

		[DataMember(Order = 1)]
		public string JcId { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("From: ").Append(From == null ? "(null)" : From.ToString());
			if (To != null && To.Count > 0)
			{
				sb.Append(" To: ").Append(To[0]);
				for (int i = 1; i < To.Count; i++)
				{
					sb.Append(", ").Append(To[i]);
				}
			}
			if (Cc != null && Cc.Count > 0)
			{
				sb.Append(" Cc: ").Append(Cc[0]);
				for (int i = 1; i < Cc.Count; i++)
				{
					sb.Append(", ").Append(Cc[i]);
				}
			}
			sb.Append(" Subject: ").Append(Subject ?? "(null)");
			return sb.ToString();
		}
	}

	[DataContract(Namespace = "http://jobctrl.com/mail", Name = "MailAddress")]
	public class MailAddress
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Email { get; set; }

		public override string ToString()
		{
			return Name + " <" + Email + ">";
		}
	}
}
