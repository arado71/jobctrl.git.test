using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.ClientComputerData
{
	public partial class ClientComputerInfoDataClassesDataContext
	{
		// ReSharper disable UnusedMember.Local
		partial void InsertClientComputerError(ClientComputerError instance)
		{
			UpsertClientComputerError(instance);
		}
		// ReSharper restore UnusedMember.Local

		public int UpsertClientComputerError(ClientComputerError obj)
		{
			int? p1 = obj.Id;
			//DateTime? p2 = obj.FirstReceiveDate;
			DateTime? p2 = null;
			var res = UpsertClientComputerError(ref p1, obj.ClientId, obj.UserId, obj.ComputerId, obj.Major, obj.Minor, obj.Build, obj.Revision, obj.Description, obj.Features, obj.HasAttachment, obj.Offset, obj.Length, obj.IsCompleted, obj.IsCancelled, ref p2);
			obj.Id = p1.GetValueOrDefault();
			obj.FirstReceiveDate = p2.GetValueOrDefault();
			return res;
		}

		public bool IsComputerUsedByUser(int userId, int computerId)
		{
			return ClientComputerInfos.Any(i => i.UserId == userId && i.ComputerId == computerId);
		}
	}
}
