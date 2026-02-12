using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.WorkTimeHistory
{
	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ClientWorkTimeHistory
	{
		[DataMember]
		public bool IsModificationApprovalNeeded { get; set; }

		[DataMember(Order = 1)]
		public TimeSpan ModificationAgeLimit { get; set; }

		[DataMember]
		public List<ComputerInterval> ComputerIntervals { get; set; }

		[DataMember]
		public List<MobileInterval> MobileIntervals { get; set; }

		[DataMember]
		public List<ManualInterval> ManualIntervals { get; set; }

		[DataMember(Order = 2)]
		public long TotalTimeInMs { get; set; }

		[DataMember(Order = 3)]
		public long? StartTimeInMs { get; set; }

		[DataMember(Order = 4)]
		public long? EndTimeInMs { get; set; }

		[DataMember(Order = 5)]
		public long? StartEndDiffInMs { get; set; }

		[DataMember(Order = 6)]
		public DateTime? LastComputerWorkitemEndTime { get; set; }
	}

	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ComputerInterval
	{
		[DataMember]
		public int WorkId { get; set; }

		[DataMember]
		public DateTime StartDate { get; set; }

		[DataMember]
		public DateTime EndDate { get; set; }

		[DataMember]
		public int ComputerId { get; set; }

		public override string ToString()
		{
			return "Start: " + StartDate.ToInvariantString()
				   + " End: " + EndDate.ToInvariantString()
				   + " WorkId: " + WorkId.ToInvariantString()
				   + " ComputerId: " + ComputerId.ToInvariantString()
				   ;
		}
	}

	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class MobileInterval
	{
		[DataMember]
		public int WorkId { get; set; }

		[DataMember]
		public DateTime StartDate { get; set; }

		[DataMember]
		public DateTime EndDate { get; set; }

		[DataMember]
		public long Imei { get; set; }

		public override string ToString()
		{
			return "Start: " + StartDate.ToInvariantString()
				   + " End: " + EndDate.ToInvariantString()
				   + " WorkId: " + WorkId.ToInvariantString()
				   + " Imei: " + Imei.ToInvariantString()
				   ;
		}
	}

	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ManualInterval
	{
		private bool? isEditableWithoutSource;
		private bool? isMeetingWithoutSource;

		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public int WorkId { get; set; } //not nullable?!

		[DataMember]
		public DateTime StartDate { get; set; }

		[DataMember]
		public DateTime EndDate { get; set; }

		[DataMember]
		public ManualWorkItemTypeEnum ManualWorkItemType { get; set; }

		[DataMember]
		public bool IsMeeting //todo shouldn't we use MeetingId.HasValue ?
		{
			get
			{
				return isMeetingWithoutSource.HasValue ? isMeetingWithoutSource.Value :
					SourceId.HasValue
					&& (((ManualWorkItemSourceEnum)SourceId.Value) == ManualWorkItemSourceEnum.MeetingAdd
						|| ((ManualWorkItemSourceEnum)SourceId.Value) == ManualWorkItemSourceEnum.ServerAdhocMeeting
						);
			}
			set { isMeetingWithoutSource = value; }
		}

		[DataMember]
		public byte? SourceId { get; set; }

		[DataMember]
		public bool IsEditable
		{
			get
			{
				return isEditableWithoutSource.HasValue ? isEditableWithoutSource.Value : 
					SourceId.HasValue
					&& (((ManualWorkItemSourceEnum)SourceId.Value) == ManualWorkItemSourceEnum.MeetingAdd
						|| ((ManualWorkItemSourceEnum)SourceId.Value) == ManualWorkItemSourceEnum.ServerAdhocMeeting
						|| ((ManualWorkItemSourceEnum)SourceId.Value) == ManualWorkItemSourceEnum.Website
						|| ((ManualWorkItemSourceEnum)SourceId.Value) == ManualWorkItemSourceEnum.ClientApiAddManual
						)
					|| IsPending; //we have no sourceId for pending meetings
			}
			set { isEditableWithoutSource = value; }
		}

		[DataMember]
		public string Comment { get; set; }

		[DataMember]
		public string Subject { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public int? MeetingId { get; set; }

		[DataMember]
		public int? PendingId { get; set; }

		[DataMember]
		public bool IsPending
		{
			get
			{
				return PendingId.HasValue;
			}
			set { }
		}

		[DataMember]
		public bool IsPendingDeleteAlso { get; set; } //for pending manuals

		public int? GetWorkId()
		{
			if (ManualWorkItemType == ManualWorkItemTypeEnum.AddHoliday || ManualWorkItemType == ManualWorkItemTypeEnum.AddSickLeave) throw new Exception("Invalid type " + ManualWorkItemType);
			return ManualWorkItemType == ManualWorkItemTypeEnum.AddWork ? WorkId : (int?)null;
		}

		public override string ToString()
		{
			return "Id: " + Id.ToInvariantString()
				   + (MeetingId.HasValue ? " MId: " + MeetingId : "")
				   + (PendingId.HasValue ? " PId: " + PendingId : "")
				   + " Start: " + StartDate.ToInvariantString()
				   + " End: " + EndDate.ToInvariantString()
				   + " WorkId: " + WorkId.ToInvariantString()
				   + " Source: " + (ManualWorkItemSourceEnum?)SourceId
				   + " Type: " + ManualWorkItemType.ToString();
		}
	}
}