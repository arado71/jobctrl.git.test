using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class EmailResource
	{
		public byte[] Data { get; set; }
		public string MediaType { get; set; }
		public string ContentId { get; set; }

		public EmailResource()
		{
		}

		public EmailResource(byte[] data, string mediaType)
		{
			ContentId = Guid.NewGuid().ToString();
			Data = data;
			MediaType = mediaType;
		}
	}
}
