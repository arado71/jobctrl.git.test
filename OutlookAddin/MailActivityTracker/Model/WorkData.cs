using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	internal partial class WorkData
	{
		[DataMember(Name = "Name", Order = 0, IsRequired = false, EmitDefaultValue = false)]
		public string Name { get; set; }
		[DataMember(Name = "Id", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public int? Id { get; set; }
		[DataMember(Name = "Type", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public int? Type { get; set; }
		[DataMember(Name = "Children", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public List<WorkData> Children { get; set; }
		[DataMember(Name = "Priority", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public int? Priority { get; set; }
		[DataMember(Name = "StartDate", Order = 5, IsRequired = false, EmitDefaultValue = false)]
		public DateTime? StartDate { get; set; }
		[DataMember(Name = "EndDate", Order = 6, IsRequired = false, EmitDefaultValue = false)]
		public DateTime? EndDate { get; set; }
		[DataMember(Name = "TargetTotalWorkTime", Order = 7, IsRequired = false, EmitDefaultValue = false)]
		public TimeSpan? TargetTotalWorkTime { get; set; }
		[DataMember(Name = "ManualAddWorkDuration", Order = 8, IsRequired = false, EmitDefaultValue = false)]
		public TimeSpan? ManualAddWorkDuration { get; set; }
		[DataMember(Name = "IsForMobile", Order = 9, IsRequired = false, EmitDefaultValue = false)]
		public bool IsForMobile { get; set; }
		[DataMember(Name = "CategoryId", Order = 10, IsRequired = false, EmitDefaultValue = false)]
		public int? CategoryId { get; set; }
		[DataMember(Name = "ProjectId", Order = 11, IsRequired = false, EmitDefaultValue = false)]
		public int? ProjectId { get; set; }
		[DataMember(Name = "ExtId", Order = 12, IsRequired = false, EmitDefaultValue = false)]
		public int? ExtId { get; set; }
		[DataMember(Name = "TaxId", Order = 13, IsRequired = false, EmitDefaultValue = false)]
		public string TaxId { get; set; }
		[DataMember(Name = "CloseReasonRequiredTime", Order = 14, IsRequired = false, EmitDefaultValue = false)]
		public TimeSpan? CloseReasonRequiredTime { get; set; }
		[DataMember(Name = "CloseReasonRequiredDate", Order = 15, IsRequired = false, EmitDefaultValue = false)]
		public DateTime? CloseReasonRequiredDate { get; set; }
		[DataMember(Name = "CloseReasonRequiredTimeRepeatInterval", Order = 16, IsRequired = false, EmitDefaultValue = false)]
		public TimeSpan? CloseReasonRequiredTimeRepeatInterval { get; set; }
		[DataMember(Name = "CloseReasonRequiredTimeRepeatCount", Order = 17, IsRequired = false, EmitDefaultValue = false)]
		public int? CloseReasonRequiredTimeRepeatCount { get; set; }
		[DataMember(Name = "ExternalWorkIdMapping", Order = 18, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, int> ExternalWorkIdMapping { get; set; }
		[DataMember(Name = "VisibilityType", Order = 19, IsRequired = false, EmitDefaultValue = false)]
		public int? VisibilityType { get; set; }
		[DataMember(Name = "Description", Order = 20, IsRequired = false, EmitDefaultValue = false)]
		public string Description { get; set; }
		[DataMember(Name = "IsDefault", Order = 21, IsRequired = false, EmitDefaultValue = false)]
		public bool IsDefault { get; set; }
		//we don't want to manage costs at the client atm.
		//[DataMember(Name = "TargetCost", Order = 21, IsRequired = false, EmitDefaultValue = false)]
		//public decimal? TargetCost { get; set; }
		[DataMember(Name = "IsReadOnly", Order = 22, IsRequired = false, EmitDefaultValue = false)]
		public bool IsReadOnly { get; set; }
		[DataMember(Name = "TemplateRegex", Order = 23, IsRequired = false, EmitDefaultValue = false)]
		public string TemplateRegex { get; set; }

		public WorkData() { }

		public WorkData(string name, int? id, int? type)
		{
			Name = name;
			Id = id;
			Type = type;
		}

		public override string ToString()
		{
			return "WorkData Id: " + Id + " Name: " + Name + " Type: " + Type;
		}

	}
}
