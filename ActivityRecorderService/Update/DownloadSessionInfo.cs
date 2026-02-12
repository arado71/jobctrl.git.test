using System;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract()]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DownloadSessionInfo
	{
		[DataMember()]
		public long ChunkCount { get; set; }
		[DataMember()]
		public Guid FileId { get; set; }
	}
}
