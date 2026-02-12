using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum ManualWorkItemTypeEnum : short
	{
		[Description("Kézzel felvitt munkaidő")]
		AddWork = 0,
		[Description("Törölt intervallum")]
		DeleteInterval = 1,
		[Description("Törölt telefonos intervallum")]
		DeleteIvrInterval = 2,
		[Description("Törölt számítógépes intervallum")]
		DeleteComputerInterval = 3,
		[Description("Szabadság")]
		AddHoliday = 4,
		[Description("Betegség")]
		AddSickLeave = 5,
		[Description("Törölt okostelefonos intervallum")]
		DeleteMobileInterval = 6,
	}

	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum ManualWorkItemSourceEnum : byte
	{
		[Description("Incident removed time")]
		IncidentRemove = 1,
		[Description("Mobile removed parallel time")]
		MobileParallelRemove = 2,
		[Description("Meeting removed parallel time")]
		MeetingParallelRemove = 3,
		[Description("Server adhoc meeting")]
		ServerAdhocMeeting = 4,
		[Description("API")]
		WebsiteApi = 5,
		[Description("Website")]
		Website = 6,
		[Description("Meeting addon")]
		MeetingAdd = 7,
		[Description("Server add workitem")]
		Server = 8,
		[Description("ClientAPI")]
		ClientApiAddManual = 9,
	}

	public static class ManualWorkItemTypeHelper
	{
		private static readonly Dictionary<ManualWorkItemTypeEnum, bool> manualWorkItemTypeEnums = new Dictionary<ManualWorkItemTypeEnum, bool>()
		{
			{ ManualWorkItemTypeEnum.AddWork, true},
			{ ManualWorkItemTypeEnum.DeleteInterval, false},
			{ ManualWorkItemTypeEnum.DeleteIvrInterval, false},
			{ ManualWorkItemTypeEnum.DeleteComputerInterval, false},
			{ ManualWorkItemTypeEnum.AddHoliday, true},
			{ ManualWorkItemTypeEnum.AddSickLeave, true},
			{ ManualWorkItemTypeEnum.DeleteMobileInterval, false},
		};

		public static bool IsWorkIdRequired(ManualWorkItemTypeEnum typeEnum)
		{
			return manualWorkItemTypeEnums[typeEnum];
		}

		public static void InitializeDbData()
		{
			ManualWorkItemSourceHelper.InitializeDbData();
			using (var context = new ManualDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var currentTypesInDb = context.ManualWorkItemTypes.ToDictionary(n => n.Id);
				var currentTypesInCode = Enum.GetValues(typeof(ManualWorkItemTypeEnum))
					.Cast<ManualWorkItemTypeEnum>()
					.Where(n => manualWorkItemTypeEnums.ContainsKey(n))
					.Select(n => new { Value = n, Name = n.Description(), IsWorkIdRequired = manualWorkItemTypeEnums[n] });
				foreach (var typeInCode in currentTypesInCode)
				{
					ManualWorkItemType typeInDb;
					if (currentTypesInDb.ContainsKey(typeInCode.Value))
					{
						typeInDb = currentTypesInDb[typeInCode.Value];
					}
					else
					{
						typeInDb = new ManualWorkItemType();
						context.ManualWorkItemTypes.InsertOnSubmit(typeInDb);
					}
					typeInDb.Id = typeInCode.Value;
					typeInDb.IsWorkIdRequired = typeInCode.IsWorkIdRequired; //updating this can throw
					typeInDb.Name = typeInCode.Name;
				}
				//we don't delete types from db
				context.SubmitChanges();
			}
		}
	}

	public class ManualWorkItemSourceHelper
	{
		public static void InitializeDbData()
		{
			using (var context = new ManualDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var currentTypesInDb = context.ManualWorkItemSources.ToDictionary(n => (ManualWorkItemSourceEnum)n.SourceId);
				var currentTypesInCode = Enum.GetValues(typeof(ManualWorkItemSourceEnum))
					.Cast<ManualWorkItemSourceEnum>()
					.Select(n => new { Value = n, Name = n.Description() });
				foreach (var typeInCode in currentTypesInCode
					.Where(n => !currentTypesInDb.ContainsKey(n.Value))  //don't update sources as this is not 'our' table
					.Select(n => new ManualWorkItemSource { SourceId = (byte)n.Value, Name = n.Name, })
					)
				{
					context.ManualWorkItemSources.InsertOnSubmit(typeInCode);
				}
				//we don't delete types from db
				context.SubmitChanges();
			}
		}
	}

	//partial class ManualWorkItemSource
	//{
	//    public ManualWorkItemSourceEnum Id //the default MSLinqToSQLGenerator doesn't seem to support nullabe enums
	//    {
	//        get { return (ManualWorkItemSourceEnum)SourceId; }
	//        set { SourceId = (byte)value; }
	//    }
	//}

	//partial class ManualWorkItem
	//{
	//    public ManualWorkItemSourceEnum? ManualWorkItemSourceId //the default MSLinqToSQLGenerator doesn't seem to support nullabe enums
	//    {
	//        get { return SourceId.HasValue ? (ManualWorkItemSourceEnum?)SourceId : null; }
	//        set { SourceId = value.HasValue ? (byte?)value : null; }
	//    }
	//}
}
