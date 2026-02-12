using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderService.Voice;

namespace Tct.ActivityRecorderService
{
	[ServiceContract(Namespace = "Tct.ActivityRecorderService.IVoiceRecorder")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface IVoiceRecorder
	{
		[OperationContract]
		AuthData Authenticate(string clientInfo);

		[OperationContract]
		void UpsertVoiceRecording(VoiceRecording voiceRecording);

		[OperationContract]
		void DeleteVoiceRecording(VoiceRecording voiceRecording);

		[OperationContract]
		ApplicationUpdateInfo GetApplicationUpdate(int userId, string application, string currentVersion);

		[OperationContract]
		byte[] GetUpdateChunk(Guid fileId, long chunkIndex);

		[OperationContract]
		ClientSetting GetClientSettings(int userId, string oldVersion, out string newVersion);

		[OperationContract]
		void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision, string application);
	}
}
