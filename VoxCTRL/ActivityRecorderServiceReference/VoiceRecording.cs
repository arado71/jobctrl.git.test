using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.ActivityRecorderServiceReference
{
	partial class VoiceRecording
	{
		public int Length { get { return Data == null ? 0 : Data.Length; } }

		public DateTime? UploadDate { get; set; }
		public bool IsMarkedForDelete { get; set; }

		[NonSerialized]
		private string status;
		public string Status
		{
			get { return status; }
			set
			{
				if (status == value) return;
				status = value;
				this.RaisePropertyChanged("Status");
			}
		}

		//cannot put [field:NonSerialized] on the PropertyChanged event... so implement Clone so we can serialize it
		public VoiceRecording Clone()
		{
			return new VoiceRecording()
			{
				UserId = UserId,
				WorkId = WorkId,
				Name = Name,
				StartDate = StartDate,
				EndDate = EndDate,
				ClientId = ClientId,
				Duration = Duration,
				Offset = Offset,
				IsMarkedForDelete = IsMarkedForDelete,
				Codec = Codec,
				Data = Data,
				Extension = Extension,
				Status = Status,
				UploadDate = UploadDate,
			};
		}

		public override string ToString()
		{
			return "Id: " + ClientId + " UserId:" + UserId + " S:" + StartDate + " D:" + Duration + " O:" + Offset + " L:" + Length + " E:" + EndDate + (IsMarkedForDelete ? " marked for delete" : "");
		}
	}
}
