using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Tct.ActivityRecorderClient.Google
{
	class GoogleDrive
	{
		internal const string ApplicationName = "JobCTRL Google Drive synchronizer";

		internal static bool IsCredentialNeeded { get; set; } = true;

		public static string GetDatasFromFileId(string fileId, out string mimeType)
		{
			if (GoogleCredentialManager.IsCredentialInitializationNeeded())
			{
				ThreadPool.QueueUserWorkItem(_ =>
				{
					GoogleCredentialManager.GetNewCredentialsIfNeeded(false, true);
				});
				mimeType = null;
				return null;
			}
			var service = new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = GoogleCredentialManager.Credential,
				ApplicationName = ApplicationName,
			});
			var path = new StringBuilder();
			FilesResource.GetRequest request = service.Files.Get(fileId);
			request.Fields = "id, name, parents, fileExtension, mimeType";
			var requestResult = request.Execute();
			mimeType = requestResult.MimeType;
			path.Append(requestResult.Name);
			path.Append(requestResult.FileExtension);

			request = service.Files.Get(requestResult.Parents[0]);
			request.Fields = "id, name, parents";
			requestResult = request.Execute();
			do
			{
				path.Insert(0, "\\");
				path.Insert(0, requestResult.Name);
				request = service.Files.Get(requestResult.Parents[0]);
				request.Fields = "id, name, parents";
				requestResult = request.Execute();
			} while (requestResult.Parents != null);

			return path.ToString();
		}
	}
}
