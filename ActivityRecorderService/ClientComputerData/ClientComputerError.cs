using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.ClientComputerData
{
	public partial class ClientComputerError : IStreamData
	{
		public int GroupId { get; set; }

		public int CompanyId { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 20)]
		public byte[] Data { get; set; }

		public string GetPath()
		{
			return ClientComputerErrorPath.Instance.GetPath(this);
		}

		public string GetUrl()
		{
			return ClientComputerErrorPath.Instance.GetUrl(this);
		}

		public int Length { get { return Data == null ? 0 : Data.Length; } }

		public string VersionString
		{
			get { return "v" + Major + "." + Minor + "." + Build + "." + Revision; }
		}
	}
}
