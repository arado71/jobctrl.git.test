using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum ParallelWorkItemTypeEnum : short
	{
		[Description("Internet Explorer Busy")]
		IEBusy = 0,
	}

	public class ParallelWorkItemTypeHelper
	{
		public static void InitializeDbData()
		{
			using (var context = new ManualDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var currentTypesInDb = context.ParallelWorkItemTypes.ToDictionary(n => n.Id);
				var currentTypesInCode = Enum.GetValues(typeof(ParallelWorkItemTypeEnum))
					.Cast<ParallelWorkItemTypeEnum>()
					.Select(n => new { Value = n, Name = n.Description() });
				foreach (var typeInCode in currentTypesInCode)
				{
					ParallelWorkItemType typeInDb;
					if (currentTypesInDb.ContainsKey(typeInCode.Value))
					{
						typeInDb = currentTypesInDb[typeInCode.Value];
					}
					else
					{
						typeInDb = new ParallelWorkItemType();
						context.ParallelWorkItemTypes.InsertOnSubmit(typeInDb);
					}
					typeInDb.Id = typeInCode.Value;
					typeInDb.Name = typeInCode.Name;
				}
				//we don't delete types from db
				context.SubmitChanges();
			}
		}
	}
}
