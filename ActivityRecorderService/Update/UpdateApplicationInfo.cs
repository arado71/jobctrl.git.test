using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ApplicationUpdateInfo : DownloadSessionInfo
	{
		[DataMember]
		public string Version { get; set; }

		public override string ToString()
		{
			return "ApplicationUpdateInfo ver: " + Version + " ch: " + ChunkCount + " fileId: " + FileId;
		}
	}
}
