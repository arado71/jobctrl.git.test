namespace Tct.ActivityRecorderService.Voice
{
	public partial class VoiceRecorderDataClassesDataContext
	{
		// ReSharper disable UnusedMember.Local
		partial void InsertVoiceRecording(VoiceRecording obj)
		{
			System.Nullable<int> p1 = obj.Id;
			this.UpsertVoiceRecordings(ref p1, ((System.Nullable<System.Guid>)(obj.ClientId)), ((System.Nullable<int>)(obj.UserId)), ((System.Nullable<int>)(obj.WorkId)), ((System.Nullable<System.DateTime>)(obj.StartDate)), ((System.Nullable<System.DateTime>)(obj.EndDate)), ((System.Nullable<int>)(obj.Duration)), ((System.Nullable<int>)(obj.Codec)), obj.Name, obj.Extension, ((System.Nullable<int>)(obj.Offset)), ((System.Nullable<int>)(obj.Length)));
			obj.Id = p1.GetValueOrDefault();
		}
		// ReSharper restore UnusedMember.Local

		public int UpsertVoiceRecording(VoiceRecording obj)
		{
			System.Nullable<int> p1 = obj.Id;
			var res = this.UpsertVoiceRecordings(ref p1, ((System.Nullable<System.Guid>)(obj.ClientId)), ((System.Nullable<int>)(obj.UserId)), ((System.Nullable<int>)(obj.WorkId)), ((System.Nullable<System.DateTime>)(obj.StartDate)), ((System.Nullable<System.DateTime>)(obj.EndDate)), ((System.Nullable<int>)(obj.Duration)), ((System.Nullable<int>)(obj.Codec)), obj.Name, obj.Extension, ((System.Nullable<int>)(obj.Offset)), ((System.Nullable<int>)(obj.Length)));
			obj.Id = p1.GetValueOrDefault();
			return res;
		}

		public int DeleteThisVoiceRecording(VoiceRecording obj)
		{
			System.Nullable<int> p1 = obj.Id;
			System.Nullable<System.DateTime> p2 = obj.DeleteDate;
			var res = this.DeleteVoiceRecordings(ref p1, ((System.Nullable<System.Guid>)(obj.ClientId)), ((System.Nullable<int>)(obj.UserId)), ref p2);
			obj.Id = p1.GetValueOrDefault();
			obj.DeleteDate = p2;
			return res;
		}
	}

	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public partial class VoiceRecording : IStreamData
	{
		public int GroupId { get; set; }

		public int CompanyId { get; set; }

		public int Length { get { return Data == null ? 0 : Data.Length; } }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 20)]
		public byte[] Data { get; set; }

		public string GetPath()
		{
			return VoiceRecordingPath.Instance.GetPath(this);
		}
	}
}
